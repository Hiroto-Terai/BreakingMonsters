using System.IO;
using System.Xml;
using UnityEditor.Android;

// Unity はゲーム画面の Activity を hardwareAccelerated="false" で書き出す。
// そのままだと広告バナー（WebView）を毎フレーム CPU で描くことになり、重い端末では入力が詰まって「応答なし」になる。
// Google Mobile Ads もハードウェアアクセラレーションを有効にするよう求めているので、ビルド時に true へ書き換える
public class AndroidHardwareAcceleration : IPostGenerateGradleAndroidProject
{
  const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
  const string UnityActivityName = "com.unity3d.player.UnityPlayerActivity";

  public int callbackOrder => 0;

  public void OnPostGenerateGradleAndroidProject(string unityLibraryPath)
  {
    string manifestPath = Path.Combine(unityLibraryPath, "src", "main", "AndroidManifest.xml");
    var manifest = new XmlDocument();
    manifest.Load(manifestPath);
    foreach (XmlElement activity in manifest.GetElementsByTagName("activity"))
    {
      if (activity.GetAttribute("name", AndroidNamespace) == UnityActivityName)
      {
        activity.SetAttribute("hardwareAccelerated", AndroidNamespace, "true");
      }
    }
    manifest.Save(manifestPath);
  }
}
