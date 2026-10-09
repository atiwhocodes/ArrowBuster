using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArrowBuster
{
    /// <summary>
    /// Boot scene entry (01 §4). Services are already installed by <see cref="GameBootstrap"/>; first launch goes
    /// straight into play (D-058), so Boot loads Gameplay. Home / World map join in M6.
    /// </summary>
    public sealed class BootLoader : MonoBehaviour
    {
        [SerializeField] private string _firstScene = "Gameplay";

        private void Start()
        {
            ServiceInstaller.EnsureInstalled();
            SceneManager.LoadScene(_firstScene);
        }
    }
}
