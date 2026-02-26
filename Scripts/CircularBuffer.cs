using System;
using UnityEngine;


namespace IngameDebugConsole
{
    /// <summary>
    /// A fixed-capacity generic ring buffer. When the buffer is full, adding a new element silently overwrites the oldest one.
    /// </summary>
    public class CircularBuffer<T>
    {
        private readonly T[] array;
        private int startIndex;


        /// <summary>Creates a fixed-capacity ring buffer with the given size.</summary>
        public CircularBuffer(int capacity)
        {
            array = new T[capacity];
        }


        /// <summary>
        /// Number of elements currently stored in the buffer (never exceeds its fixed capacity).
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// Returns the element at the given logical index (0 is the oldest element).
        /// </summary>
        public T this[int index] => array[(startIndex + index) % array.Length];


        // Old elements are overwritten when capacity is reached
        /// <summary>Adds a value to the buffer, overwriting the oldest element when capacity is full.</summary>
        public void Add(T value)
        {
            if (Count < array.Length)
            {
                array[Count++] = value;
            }
            else
            {
                array[startIndex] = value;

                if (++startIndex >= array.Length)
                {
                    startIndex = 0;
                }
            }
        }
    }


    /// <summary>
    /// A generic ring buffer that grows its internal array automatically when the element count exceeds capacity.
    /// </summary>
    public class DynamicCircularBuffer<T>
    {
        private T[] array;
        private int startIndex;


        /// <summary>Creates a dynamic ring buffer with the specified initial backing-array size.</summary>
        public DynamicCircularBuffer(int initialCapacity = 2)
        {
            array = new T[initialCapacity];
        }


        /// <summary>
        /// Number of elements currently stored in the buffer.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Current size of the internal backing array.
        /// </summary>
        public int Capacity => array.Length;

        /// <summary>
        /// Gets or sets the element at the given logical index (0 is the oldest element).
        /// </summary>
        public T this[int index]
        {
            get => array[(startIndex + index) % array.Length];
            set => array[(startIndex + index) % array.Length] = value;
        }


        private void SetCapacity(int capacity)
        {
            var newArray = new T[capacity];

            if (Count > 0)
            {
                var elementsBeforeWrap = Mathf.Min(Count, array.Length - startIndex);
                Array.Copy(array, startIndex, newArray, 0, elementsBeforeWrap);

                if (elementsBeforeWrap < Count)
                {
                    Array.Copy(array, 0, newArray, elementsBeforeWrap, Count - elementsBeforeWrap);
                }
            }

            array = newArray;
            startIndex = 0;
        }


        /// <summary>Inserts the value to the beginning of the collection.</summary>
        public void AddFirst(T value)
        {
            if (array.Length == Count)
            {
                SetCapacity(Mathf.Max(array.Length * 2, 4));
            }

            startIndex = startIndex > 0 ? startIndex - 1 : array.Length - 1;
            array[startIndex] = value;
            Count++;
        }


        /// <summary>Adds the value to the end of the collection.</summary>
        public void Add(T value)
        {
            if (array.Length == Count)
            {
                SetCapacity(Mathf.Max(array.Length * 2, 4));
            }

            this[Count++] = value;
        }


        /// <summary>Appends all elements from <paramref name="other"/> to the end of this buffer, growing if necessary.</summary>
        public void AddRange(DynamicCircularBuffer<T> other)
        {
            if (other.Count == 0)
            {
                return;
            }

            if (array.Length < Count + other.Count)
            {
                SetCapacity(Mathf.Max(array.Length * 2, Count + other.Count));
            }

            var insertStartIndex = (startIndex + Count) % array.Length;
            var elementsBeforeWrap = Mathf.Min(other.Count, array.Length - insertStartIndex);
            var otherElementsBeforeWrap = Mathf.Min(other.Count, other.array.Length - other.startIndex);

            Array.Copy(other.array, other.startIndex, array, insertStartIndex, Mathf.Min(elementsBeforeWrap, otherElementsBeforeWrap));

            if (elementsBeforeWrap < otherElementsBeforeWrap) // This array wrapped before the other array
            {
                Array.Copy(other.array, other.startIndex + elementsBeforeWrap, array, 0, otherElementsBeforeWrap - elementsBeforeWrap);
            }
            else if (elementsBeforeWrap > otherElementsBeforeWrap) // The other array wrapped before this array
            {
                Array.Copy(other.array, 0, array, insertStartIndex + otherElementsBeforeWrap, elementsBeforeWrap - otherElementsBeforeWrap);
            }

            var copiedElements = Mathf.Max(elementsBeforeWrap, otherElementsBeforeWrap);

            if (copiedElements < other.Count) // Both arrays wrapped and there's still some elements left to copy
            {
                Array.Copy(other.array, copiedElements - otherElementsBeforeWrap, array, copiedElements - elementsBeforeWrap, other.Count - copiedElements);
            }

            Count += other.Count;
        }


        /// <summary>Removes and returns the first element, advancing the start index.</summary>
        public T RemoveFirst()
        {
            var element = array[startIndex];
            array[startIndex] = default;

            if (++startIndex == array.Length)
            {
                startIndex = 0;
            }

            Count--;

            return element;
        }


        /// <summary>Removes and returns the last element.</summary>
        public T RemoveLast()
        {
            var index = (startIndex + Count - 1) % array.Length;
            var element = array[index];
            array[index] = default;

            Count--;

            return element;
        }


        /// <summary>Removes all elements that satisfy <paramref name="shouldRemoveElement"/>, compacting the buffer in place.</summary>
        public int RemoveAll(Predicate<T> shouldRemoveElement)
        {
            return RemoveAll<T>(shouldRemoveElement, null, null);
        }


        /// <summary>
        /// Removes all matching elements while optionally notifying their new indices and keeping a second buffer in sync.
        /// </summary>
        public int RemoveAll<Y>(Predicate<T> shouldRemoveElement, Action<T, int> onElementIndexChanged, DynamicCircularBuffer<Y> synchronizedBuffer)
        {
            var synchronizedArray = synchronizedBuffer != null ? synchronizedBuffer.array : null;
            var elementsBeforeWrap = Mathf.Min(Count, array.Length - startIndex);
            var removedElements = 0;
            int i = startIndex, newIndex = startIndex, endIndex = startIndex + elementsBeforeWrap;

            for (; i < endIndex; i++)
            {
                if (shouldRemoveElement(array[i]))
                {
                    removedElements++;
                }
                else
                {
                    if (removedElements > 0)
                    {
                        var element = array[i];
                        array[newIndex] = element;

                        if (synchronizedArray != null)
                        {
                            synchronizedArray[newIndex] = synchronizedArray[i];
                        }

                        if (onElementIndexChanged != null)
                        {
                            onElementIndexChanged(element, newIndex - startIndex);
                        }
                    }

                    newIndex++;
                }
            }

            i = 0;
            endIndex = Count - elementsBeforeWrap;

            if (newIndex < array.Length)
            {
                for (; i < endIndex; i++)
                {
                    if (shouldRemoveElement(array[i]))
                    {
                        removedElements++;
                    }
                    else
                    {
                        var element = array[i];
                        array[newIndex] = element;

                        if (synchronizedArray != null)
                        {
                            synchronizedArray[newIndex] = synchronizedArray[i];
                        }

                        if (onElementIndexChanged != null)
                        {
                            onElementIndexChanged(element, newIndex - startIndex);
                        }

                        if (++newIndex == array.Length)
                        {
                            i++;

                            break;
                        }
                    }
                }
            }

            if (newIndex == array.Length)
            {
                newIndex = 0;

                for (; i < endIndex; i++)
                {
                    if (shouldRemoveElement(array[i]))
                    {
                        removedElements++;
                    }
                    else
                    {
                        if (removedElements > 0)
                        {
                            var element = array[i];
                            array[newIndex] = element;

                            if (synchronizedArray != null)
                            {
                                synchronizedArray[newIndex] = synchronizedArray[i];
                            }

                            if (onElementIndexChanged != null)
                            {
                                onElementIndexChanged(element, newIndex + elementsBeforeWrap);
                            }
                        }

                        newIndex++;
                    }
                }
            }

            TrimEnd(removedElements);

            synchronizedBuffer?.TrimEnd(removedElements);

            return removedElements;
        }


        /// <summary>Removes the first <paramref name="trimCount"/> elements, invoking an optional per-element callback before each removal.</summary>
        public void TrimStart(int trimCount, Action<T> perElementCallback = null)
        {
            TrimInternal(trimCount, startIndex, perElementCallback);
            startIndex = (startIndex + trimCount) % array.Length;
        }


        /// <summary>Removes the last <paramref name="trimCount"/> elements, invoking an optional per-element callback before each removal.</summary>
        public void TrimEnd(int trimCount, Action<T> perElementCallback = null)
        {
            TrimInternal(trimCount, (startIndex + Count - trimCount) % array.Length, perElementCallback);
        }


        private void TrimInternal(int trimCount, int startIndex, Action<T> perElementCallback)
        {
            var elementsBeforeWrap = Mathf.Min(trimCount, array.Length - startIndex);

            if (perElementCallback == null)
            {
                Array.Clear(array, startIndex, elementsBeforeWrap);

                if (elementsBeforeWrap < trimCount)
                {
                    Array.Clear(array, 0, trimCount - elementsBeforeWrap);
                }
            }
            else
            {
                for (int i = startIndex, endIndex = startIndex + elementsBeforeWrap; i < endIndex; i++)
                {
                    perElementCallback(array[i]);
                    array[i] = default;
                }

                for (int i = 0, endIndex = trimCount - elementsBeforeWrap; i < endIndex; i++)
                {
                    perElementCallback(array[i]);
                    array[i] = default;
                }
            }

            Count -= trimCount;
        }


        /// <summary>
        /// Removes all elements from the buffer.
        /// </summary>
        public void Clear()
        {
            var elementsBeforeWrap = Mathf.Min(Count, array.Length - startIndex);
            Array.Clear(array, startIndex, elementsBeforeWrap);

            if (elementsBeforeWrap < Count)
            {
                Array.Clear(array, 0, Count - elementsBeforeWrap);
            }

            startIndex = 0;
            Count = 0;
        }


        /// <summary>
        /// Returns the logical index of the first occurrence of <paramref name="value"/>, or -1 if not found.
        /// </summary>
        public int IndexOf(T value)
        {
            var elementsBeforeWrap = Mathf.Min(Count, array.Length - startIndex);
            var index = Array.IndexOf(array, value, startIndex, elementsBeforeWrap);

            if (index >= 0)
            {
                return index - startIndex;
            }

            if (elementsBeforeWrap < Count)
            {
                index = Array.IndexOf(array, value, 0, Count - elementsBeforeWrap);

                if (index >= 0)
                {
                    return index + elementsBeforeWrap;
                }
            }

            return -1;
        }


        /// <summary>
        /// Invokes <paramref name="action"/> once for each element in the buffer, in logical order.
        /// </summary>
        public void ForEach(Action<T> action)
        {
            var elementsBeforeWrap = Mathf.Min(Count, array.Length - startIndex);

            for (int i = startIndex, endIndex = startIndex + elementsBeforeWrap; i < endIndex; i++)
            {
                action(array[i]);
            }

            for (int i = 0, endIndex = Count - elementsBeforeWrap; i < endIndex; i++)
            {
                action(array[i]);
            }
        }
    }
}
