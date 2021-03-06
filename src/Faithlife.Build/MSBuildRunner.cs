using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Faithlife.Build.AppRunner;

namespace Faithlife.Build
{
	/// <summary>
	/// Runs MSBuild.
	/// </summary>
	/// <remarks>
	/// <para>Consider calling these methods directly via <c>using static Faithlife.Build.MSBuildRunner;</c></para>
	/// </remarks>
	public static class MSBuildRunner
	{
		/// <summary>
		/// Gets the path of MSBuild for the specified version.
		/// </summary>
		public static string GetMSBuildPath(MSBuildSettings? settings)
		{
			if (settings?.MSBuildPath is string settingsPath)
				return settingsPath;

			if (BuildEnvironment.IsMacOS())
			{
				const string msbuildPath = "/Library/Frameworks/Mono.framework/Versions/Current/Commands/msbuild";
				if (File.Exists(msbuildPath))
					return msbuildPath;
			}
			else if (BuildEnvironment.IsLinux())
			{
				const string msbuildPath = "/usr/bin/msbuild";
				if (File.Exists(msbuildPath))
					return msbuildPath;
			}
			else
			{
				var version = settings?.Version ?? MSBuildVersion.VS2017;
				var platform = settings?.Platform ?? (BuildEnvironment.Is64Bit() ? MSBuildPlatform.X64 : MSBuildPlatform.X32);

				(string Year, string Version) getPathParts()
				{
					if (version == MSBuildVersion.VS2017)
						return ("2017", "15.0");
					else if (version == MSBuildVersion.VS2019)
						return ("2019", "Current");
					else
						throw new ArgumentException("Invalid version.", nameof(version));
				}

				var parts = getPathParts();
				foreach (var edition in new[] { "Enterprise", "Professional", "Community", "BuildTools", "Preview" })
				{
					var msbuildPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
						"Microsoft Visual Studio", parts.Year, edition, "MSBuild", parts.Version, "Bin", platform == MSBuildPlatform.X64 ? "amd64" : "", "MSBuild.exe");
					if (File.Exists(msbuildPath))
						return msbuildPath;
				}
			}

			throw new InvalidOperationException("MSBuild not found.");
		}

		/// <summary>
		/// Runs MSBuild with the specified arguments.
		/// </summary>
		/// <param name="settings">The MSBuild settings.</param>
		/// <param name="args">The arguments, if any.</param>
		public static void RunMSBuild(MSBuildSettings? settings, params string?[] args)
		{
			if (args == null)
				throw new ArgumentNullException(nameof(args));

			RunMSBuild(settings, args.AsEnumerable());
		}

		/// <summary>
		/// Runs MSBuild with the specified arguments.
		/// </summary>
		/// <param name="settings">The MSBuild settings.</param>
		/// <param name="args">The arguments, if any.</param>
		public static void RunMSBuild(MSBuildSettings? settings, IEnumerable<string?> args) => RunApp(GetMSBuildPath(settings), args);

		/// <summary>
		/// Runs MSBuild with the specified settings.
		/// </summary>
		/// <param name="settings">The MSBuild settings.</param>
		/// <param name="runnerSettings">The settings to use when running the app.</param>
		public static int RunMSBuild(MSBuildSettings? settings, AppRunnerSettings runnerSettings) => RunApp(GetMSBuildPath(settings), runnerSettings);
	}
}
