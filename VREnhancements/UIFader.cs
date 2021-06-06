using UnityEngine;
using System.Collections;


namespace VREnhancements
{
    class UIFader : MonoBehaviour
    {
        bool fading = false;
        CanvasGroup cg;
        Coroutine fadeCR;
        
        void Start()
        {
            if (!this.gameObject.GetComponent<CanvasGroup>())
               cg = this.gameObject.AddComponent<CanvasGroup>();
        }
        public void Fade(float targetAlpha, float fadeSpeed = 1)
        {
            //if already fading, stop the current fade before starting a new one.
            if (fading)
                StopCoroutine(fadeCR);
            fadeCR = StartCoroutine(FadeCG(targetAlpha, fadeSpeed));
        }
        IEnumerator FadeCG(float targetAlpha, float fadeSpeed)
        {
            if (cg)
            {
                float newAlpha = cg.alpha;
                while (newAlpha != targetAlpha)
                {
                    fading = true;
                    newAlpha = Mathf.MoveTowards(newAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
                    cg.alpha = newAlpha;
                    yield return null;
                }
            }
            fading = false;//fade completed
        }
    }
}
