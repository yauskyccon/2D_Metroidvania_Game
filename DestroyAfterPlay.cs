using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class DestroyAfterPlay : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        // Add a callback for when the video finishes
        videoPlayer.loopPointReached += OnVideoFinished;
    }
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        Destroy(gameObject);
    }
}
