using UnityEngine;
using UnityEngine.Video;

public class VideoSequence : MonoBehaviour
{
    public VideoPlayer firstPlayer;
    public VideoPlayer secondPlayer;

    void Start()
    {
        // 시작 시 상태 고정
        firstPlayer.gameObject.SetActive(true);
        secondPlayer.gameObject.SetActive(false);

        firstPlayer.isLooping = false;
        secondPlayer.isLooping = true;

        // 첫 번째 끝났을 때만 두 번째로 전환
        firstPlayer.loopPointReached += OnFirstEnded;

        // 두 번째 영상 미리 준비
        secondPlayer.Prepare();

        // 첫 번째 영상 시작
        firstPlayer.Play();

        Debug.Log("첫 번째 영상 시작");
    }

    void OnFirstEnded(VideoPlayer vp)
    {
        Debug.Log("첫 번째 영상 끝, 두 번째 영상 시작");

        firstPlayer.gameObject.SetActive(false);
        secondPlayer.gameObject.SetActive(true);
        secondPlayer.Play();
    }
}