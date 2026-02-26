#if IDG_ENABLE_HELPER_COMMANDS
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IngameDebugConsole.Commands
{
	/// <summary>
	///     Built-in console commands for scene management. Registered under the <c>IDG_ENABLE_HELPER_COMMANDS</c> scripting define.
	/// </summary>
	public class SceneCommands
	{
		/// <summary>Loads <paramref name="sceneName" /> synchronously, replacing the current scene.</summary>
		[ConsoleMethod( "scene.load", "Loads a scene" ), UnityEngine.Scripting.Preserve]
		public static void LoadScene( string sceneName )
		{
			LoadSceneInternal( sceneName, false, LoadSceneMode.Single );
		}

		/// <summary>Loads <paramref name="sceneName" /> synchronously using the specified <paramref name="mode" />.</summary>
		[ConsoleMethod( "scene.load", "Loads a scene" ), UnityEngine.Scripting.Preserve]
		public static void LoadScene( string sceneName, LoadSceneMode mode )
		{
			LoadSceneInternal( sceneName, false, mode );
		}

		/// <summary>Loads <paramref name="sceneName" /> asynchronously, replacing the current scene.</summary>
		[ConsoleMethod( "scene.loadasync", "Loads a scene asynchronously" ), UnityEngine.Scripting.Preserve]
		public static void LoadSceneAsync( string sceneName )
		{
			LoadSceneInternal( sceneName, true, LoadSceneMode.Single );
		}

		/// <summary>Loads <paramref name="sceneName" /> asynchronously using the specified <paramref name="mode" />.</summary>
		[ConsoleMethod( "scene.loadasync", "Loads a scene asynchronously" ), UnityEngine.Scripting.Preserve]
		public static void LoadSceneAsync( string sceneName, LoadSceneMode mode )
		{
			LoadSceneInternal( sceneName, true, mode );
		}

		private static void LoadSceneInternal( string sceneName, bool isAsync, LoadSceneMode mode )
		{
			if( SceneManager.GetSceneByName( sceneName ).IsValid() )
			{
				Debug.Log( "Scene " + sceneName + " is already loaded" );
				return;
			}

			if( isAsync )
				SceneManager.LoadSceneAsync( sceneName, mode );
			else
				SceneManager.LoadScene( sceneName, mode );
		}

		/// <summary>Unloads <paramref name="sceneName" /> asynchronously.</summary>
		[ConsoleMethod( "scene.unload", "Unloads a scene" ), UnityEngine.Scripting.Preserve]
		public static void UnloadScene( string sceneName )
		{
			SceneManager.UnloadSceneAsync( sceneName );
		}

		/// <summary>Reloads the currently active scene in <see cref="UnityEngine.SceneManagement.LoadSceneMode.Single" /> mode.</summary>
		[ConsoleMethod( "scene.restart", "Restarts the active scene" ), UnityEngine.Scripting.Preserve]
		public static void RestartScene()
		{
			SceneManager.LoadScene( SceneManager.GetActiveScene().name, LoadSceneMode.Single );
		}
	}
}
#endif