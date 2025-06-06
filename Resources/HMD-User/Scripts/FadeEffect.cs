/*
Credits to SunnyValleyStudio
https://github.com/SunnyValleyStudio/Unity-VR-Prevent-Head-Clipping-with-Player-Push-Back-and-Fade-effect
https://www.youtube.com/watch?v=YRjfmblMj8Q

MIT License

Copyright (c) 2023 Peter

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRSYS.Core.Navigation;

public class FadeEffect : MonoBehaviour
{
    //public Camera overlayCamera;
    
    [SerializeField]
    private float _fadeDelay = 1.5f;
    private Material _material;

    private bool _isFadingOut = false;

    private void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    public void FadeIn()
    {
        //overlayCamera.enabled = true;
        this.Fade(true);
    }

    public void FadeOut()
    {
        this.Fade(false);
    }

    public void FadeComplete()
    {
        //overlayCamera.enabled = false;
    }

    public void SetFadeDelay(float delay)
    {
        _fadeDelay = delay;
    }

    public void Fade(bool fadeOut)
    {
        if (fadeOut && _isFadingOut)
            return;
        if (!fadeOut && !_isFadingOut)
            return;
        _isFadingOut = fadeOut;
        StopAllCoroutines();
        string val = _isFadingOut ? "OUT" : "in";
        Debug.Log($"Starting fade {val} coroutine");
        StartCoroutine(PlayEffect(fadeOut));
    }

    private IEnumerator PlayEffect(bool fadeOut)
    {
        float startAlpha = _material.GetFloat("_Alpha");
        float endAlpha = fadeOut ? 1.0f : 0.0f;
        float remainingTime
            = _fadeDelay * Mathf.Abs(endAlpha - startAlpha);

        float elapsedTime = 0;
        while (elapsedTime < _fadeDelay)
        {
            elapsedTime += Time.deltaTime;
            float tempVal = Mathf.Lerp(startAlpha, endAlpha,
                elapsedTime / remainingTime);

            _material.SetFloat("_Alpha", tempVal);
            yield return null;
        }
        _material.SetFloat("_Alpha", endAlpha);
    }
}
