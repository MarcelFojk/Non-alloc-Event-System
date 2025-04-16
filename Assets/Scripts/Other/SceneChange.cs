using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Other
{
    public class SceneChange : MonoBehaviour
    {
        private void Awake()
        {
            StartCoroutine(ChangeScene(1f));
        }

        private IEnumerator ChangeScene(float time)
        {
            yield return new WaitForSeconds(time);
            SceneManager.LoadScene("TestScene2");
        }
    }
}