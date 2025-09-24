using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void Load_SinglePlayer() => SceneManager.LoadScene(1);
    public void Load_MultiPlayer()  => SceneManager.LoadScene(2);
}
