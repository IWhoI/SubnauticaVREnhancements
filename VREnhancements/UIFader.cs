using UnityEngine;
using System.Collections;


namespace VREnhancements
{
    class UIFader : MonoBehaviour
    {
        CanvasGroup cg;
        Coroutine fadeCR;
        bool fading = false;
        void Start()
        {
            if (!this.gameObject.GetComponent<CanvasGroup>())
               cg = this.gameObject.AddComponent<CanvasGroup>();
        }
        
        public void Fade(float targetAlpha, float fadeSpeed = 1, float delaySeconds = 0, bool reset = false)
        {
            ErrorMessage.AddMessage("ta: " + targetAlpha + " speed: " + fadeSpeed + " delay: " + delaySeconds + " reset: " + reset);
            //if currently fading and reset true, stop current fade and start new fade
            if (fadeCR != null && fading && reset)
            {
                StopCoroutine(fadeCR);
                fadeCR = StartCoroutine(FadeCG(targetAlpha, fadeSpeed, delaySeconds));
            }
            else if (!fading)
                fadeCR = StartCoroutine(FadeCG(targetAlpha, fadeSpeed, delaySeconds));
        }
        IEnumerator FadeCG(float targetAlpha, float fadeSpeed, float seconds)
        {
            if (cg)
            {
                fading = true;
                if(seconds > 0)
                    yield return new WaitForSeconds(seconds);
                float newAlpha = cg.alpha;
                if (fadeSpeed <= 0)
                    cg.alpha = targetAlpha;
                else
                    while (newAlpha != targetAlpha)
                    {
                        newAlpha = Mathf.MoveTowards(newAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
                        cg.alpha = newAlpha;
                        yield return null;
                    }
                fading = false;
            }
        }
    }
}
