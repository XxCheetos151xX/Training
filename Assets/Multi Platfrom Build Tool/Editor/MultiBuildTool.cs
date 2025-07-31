using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using Unity.EditorCoroutines.Editor;
using System.IO;

namespace MPBT
{
    public class MultiBuildTool : EditorWindow
    {
        [LargeTextInGUI]
        public int Headline;
        private const float WaitTimeSeconds = 1f;
        private const string BuildsDirectoryName = "Builds";

        private Dictionary<BuildTarget, bool> targetsToBuild = new Dictionary<BuildTarget, bool>();
        private List<BuildTarget> availableTargets = new List<BuildTarget>();

        private static Dictionary<BuildTarget, BuildTargetGroup> targetToGroupMap = new Dictionary<BuildTarget, BuildTargetGroup>
    {
        { BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone },
        { BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone },
        { BuildTarget.iOS, BuildTargetGroup.iOS },
        { BuildTarget.Android, BuildTargetGroup.Android },
        { BuildTarget.StandaloneWindows64, BuildTargetGroup.Standalone },
        { BuildTarget.WebGL, BuildTargetGroup.WebGL },
        { BuildTarget.StandaloneLinux64, BuildTargetGroup.Standalone }
    };

        [MenuItem("File/Multi-Platform Build/Build")]
        public static void OnShowTools()
        {
            EditorWindow.GetWindow<MultiBuildTool>();
        }

        private void OnEnable()
        {
            availableTargets.Clear();
            foreach (var buildTarget in System.Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTarget target = (BuildTarget)buildTarget;
                if (!BuildPipeline.IsBuildTargetSupported(GetTargetGroupForTarget(target), target))
                    continue;

                availableTargets.Add(target);

                if (!targetsToBuild.ContainsKey(target))
                    targetsToBuild[target] = false;
            }

            RemoveUnavailableTargets();
        }

        private void RemoveUnavailableTargets()
        {
            List<BuildTarget> targetsToRemove = new List<BuildTarget>();
            foreach (var target in targetsToBuild.Keys)
            {
                if (!availableTargets.Contains(target))
                    targetsToRemove.Add(target);
            }

            foreach (var target in targetsToRemove)
                targetsToBuild.Remove(target);
        }

        private BuildTargetGroup GetTargetGroupForTarget(BuildTarget target)
        {
            if (targetToGroupMap.TryGetValue(target, out BuildTargetGroup group))
                return group;

            return BuildTargetGroup.Unknown;
        }
        private void OnGUI()
        {
            GUIStyle headStyle = new GUIStyle();
            headStyle.fontSize = 30;
            headStyle.wordWrap = true;
            headStyle.fontStyle = FontStyle.Bold;
            headStyle.normal.textColor = Color.white;
            GUILayout.Label("Platforms to Build", headStyle);
            GUILayout.Space(20);

            int numEnabled = 0;
            foreach (var target in availableTargets)
            {
                targetsToBuild[target] = EditorGUILayout.Toggle(target.ToString(), targetsToBuild[target]);

                if (targetsToBuild[target])
                    numEnabled++;
            }

            if (numEnabled > 0)
            {
                string prompt = numEnabled == 1 ? "Build 1 Platform" : $"Build {numEnabled} Platforms";
                if (GUILayout.Button(prompt))
                {
                    List<BuildTarget> selectedTargets = new List<BuildTarget>();
                    foreach (var target in availableTargets)
                    {
                        if (targetsToBuild[target])
                            selectedTargets.Add(target);
                    }

                    EditorCoroutineUtility.StartCoroutine(PerformBuild(selectedTargets), this);
                }
            }
            GUILayout.Space(100);
            GUIStyle Des = new GUIStyle();
            Des.fontSize = 30;
            Des.wordWrap = true;
            Des.fontStyle = FontStyle.Bold;
            Des.normal.textColor = Color.white;
            GUILayout.Label("How To Use", Des);

            GUIStyle content = new GUIStyle();
            content.fontSize = 15;
            content.wordWrap = true;
            content.fontStyle = FontStyle.Normal;
            content.normal.textColor = Color.white;
            GUILayout.Label("The Package Will Detect All Build Target Installed On The Machine And Will Preview Them To You, ", content);
            GUILayout.Label("Then You Can Choose Which Build Target You Want and Select Build Platforms.", content);
        }

        private IEnumerator PerformBuild(List<BuildTarget> targetsToBuild)
        {
            int buildAllProgressID = Progress.Start("Build All", "Building all selected platforms", Progress.Options.Sticky);
            Progress.ShowDetails();
            yield return new EditorWaitForSeconds(WaitTimeSeconds);

            BuildTarget originalTarget = EditorUserBuildSettings.activeBuildTarget;

            for (int targetIndex = 0; targetIndex < targetsToBuild.Count; ++targetIndex)
            {
                var buildTarget = targetsToBuild[targetIndex];

                Progress.Report(buildAllProgressID, targetIndex + 1, targetsToBuild.Count);
                int buildTaskProgressID = Progress.Start($"Build {buildTarget.ToString()}", null, Progress.Options.Sticky, buildAllProgressID);
                yield return new EditorWaitForSeconds(WaitTimeSeconds);

                if (!BuildIndividualTarget(buildTarget))
                {
                    Progress.Finish(buildTaskProgressID, Progress.Status.Failed);
                    Progress.Finish(buildAllProgressID, Progress.Status.Failed);

                    if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
                        EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetTargetGroupForTarget(originalTarget), originalTarget);

                    yield break;
                }

                Progress.Finish(buildTaskProgressID, Progress.Status.Succeeded);
                yield return new EditorWaitForSeconds(WaitTimeSeconds);
            }

            Progress.Finish(buildAllProgressID, Progress.Status.Succeeded);

            if (EditorUserBuildSettings.activeBuildTarget != originalTarget)
                EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetTargetGroupForTarget(originalTarget), originalTarget);

            yield return null;
        }

        private bool BuildIndividualTarget(BuildTarget target)
        {
            BuildPlayerOptions options = new BuildPlayerOptions();
            List<string> scenes = new List<string>();

            foreach (var scene in EditorBuildSettings.scenes)
                scenes.Add(scene.path);

            options.scenes = scenes.ToArray();
            options.target = target;
            options.targetGroup = GetTargetGroupForTarget(target);

            string locationPath = GetBuildLocationPath(target);
            options.locationPathName = locationPath;

            // Add BuildOptions.SymlinkLibraries for Windows builds
            if (target == BuildTarget.StandaloneWindows64)
                options.options |= BuildOptions.SymlinkSources;

            if (BuildPipeline.BuildCanBeAppended(target, options.locationPathName) == CanAppendBuild.Yes)
                options.options |= BuildOptions.AcceptExternalModificationsToPlayer;

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build for {target.ToString()} completed in {report.summary.totalTime.Seconds} seconds");
                return true;
            }

            Debug.LogError($"Build for {target.ToString()} failed");
            return false;
        }

        private string GetBuildLocationPath(BuildTarget target)
        {
            string extension = "";

            if (target == BuildTarget.Android)
                extension = ".apk";
            else if (target == BuildTarget.StandaloneWindows)
                extension = ".exe";
            else if (target == BuildTarget.StandaloneLinux64)
                extension = ".x86_64";

            string fileName = PlayerSettings.productName + extension;
            string directoryPath = Path.Combine(BuildsDirectoryName, target.ToString());

            // Create the directory if it doesn't exist
            Directory.CreateDirectory(directoryPath);

            return Path.Combine(directoryPath, fileName);
        }
    }

    [CustomPropertyDrawer(typeof(LargeTextInGUI))]
    public class LargeTextInGUIDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var style = new GUIStyle();
            style.fontSize = 40;
            EditorGUI.LabelField(position, label.text, property.intValue.ToString(), style);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 45;
        }
    }

    public class LargeTextInGUI : PropertyAttribute
    {
    }
}