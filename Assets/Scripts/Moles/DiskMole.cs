﻿using Assets.Scripts.Game;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using UnityEngine;

/*
An implementation of the Mole abstract class. Defines different parameters to modify and
overrides the actions to do on different events (enables, disables, popped...).
*/

public class DiskMole : Mole
{
    [SerializeField]
    private Color disabledColor;

    [SerializeField]
    private Color enabledColor;

    [SerializeField]
    private Color popFast;

    [SerializeField]
    private Color popSlow;

    [SerializeField]
    private Color fakeEnabledColor;

    [SerializeField]
    private Color hoverColor;

    [SerializeField]
    private Color fakeHoverColor;

    [SerializeField]
    private Color popColor;

    [SerializeField]
    private Texture textureEnabled;

    [SerializeField]
    private Texture textureDisabled;

    [SerializeField]
    private Texture distractorLeftTexture;

    [SerializeField]
    private Texture distractorRightTexture;

    [SerializeField]
    private Texture correctMoleTexture;

    [SerializeField]
    private Texture incorrectMoleTexture;

    [SerializeField]
    private AudioClip enableSound;

    [SerializeField]
    private AudioClip disableSound;

    [SerializeField]
    private AudioClip popSound;

    private Shader opaqueShader;
    private Shader glowShader;
    private Material meshMaterial;
    private AudioSource audioSource;
    private Animation animationPlayer;
    private Coroutine colorAnimation;
    private string playingClip = "";

    protected override void Start()
    {
        animationPlayer = gameObject.GetComponent<Animation>();
        meshMaterial = gameObject.GetComponentInChildren<Renderer>().material;
        opaqueShader = Shader.Find("Standard");
        glowShader = Shader.Find("Particles/Standard Unlit");
        audioSource = gameObject.GetComponent<AudioSource>();
        PlayAnimation("EnableDisable");
        meshMaterial.color = disabledColor;

        base.Start();

    }

    private void Update()
    {
    }
    //public void EndPlayPop()
    //{
    //    base.PlayPop();
    //}

    /*
    Override of the event functions of the base class.
    */

    protected override void PlayEnabling()
    {
        PlaySound(enableSound);
        PlayAnimation("EnableDisable");

        if (moleType == Mole.MoleType.Target)
        {
              meshMaterial.color = enabledColor;
              meshMaterial.mainTexture = textureEnabled;
        }
        else if (moleType == Mole.MoleType.DistractorLeft)
        {
            meshMaterial.color = fakeEnabledColor;
            meshMaterial.mainTexture =  distractorLeftTexture;
        } else if (moleType == Mole.MoleType.DistractorRight)
        {
            meshMaterial.color = fakeEnabledColor;
            meshMaterial.mainTexture =  distractorRightTexture;
        }
        base.PlayEnabling();
    }

    protected override void PlayDisabling()
    {
        PlaySound(enableSound);
        PlayAnimation("EnableDisable"); // Don't show any feedback to users when an incorrect moles expires
        meshMaterial.color=disabledColor;
        meshMaterial.mainTexture=textureDisabled;
        base.PlayDisabling();
    }

    protected override void PlayMissed()
    {
        meshMaterial.color=disabledColor;
        meshMaterial.mainTexture=textureDisabled;
        if (ShouldPerformanceFeedback()) {
            PlayAnimation("PopWrongMole"); // Show negative feedback to users when a correct moles expires, to make it clear that they missed it
        }
        base.PlayMissed();
    }

    protected override void PlayHoverEnter()
    {
        if (moleType == Mole.MoleType.Target)
        {
            meshMaterial.color = hoverColor;
        }
        else
        {
            meshMaterial.color = fakeHoverColor;
        }
    }

    protected override void PlayHoverLeave()
    {
        if (moleType == Mole.MoleType.Target)
        {
            meshMaterial.color = enabledColor;
        }
        else
        {
            meshMaterial.color = fakeEnabledColor;
        }
    }

    protected override void PlayPop(float feedback)
    {
        Debug.Log("SouldPerformanceFeedback : " + ShouldPerformanceFeedback());
        if (ShouldPerformanceFeedback()) {
            if (moleType==Mole.MoleType.Target)
            {
                Color colorFeedback = Color.Lerp(popSlow, popFast, feedback);
                StartCoroutine(ChangeColorOverTime(enabledColor, colorFeedback, disabledColor, 0.2f, 0.3f));
                meshMaterial.mainTexture = textureDisabled;
            }
            else
            {
                PlayAnimation("PopWrongMole");    // Show negative feedback to users that shoot an incorrect moles, to make it clear this is a fail
                meshMaterial.color = disabledColor;
                meshMaterial.mainTexture = textureDisabled;
            }
        }
        PlaySound(popSound);
        //base.PlayPop(); // we cannot change to popped state, this breaks WAIT:HIT for some reason.
    }

    protected override void PlayReset()
    {
        PlayAnimation("EnableDisable");
        meshMaterial.color = disabledColor;
        meshMaterial.mainTexture = textureDisabled;
    }
    IEnumerator ChangeColorOverTime(Color colorStart, Color colorFeedback, Color colorEnd, float duration, float waitTime)
    {
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            meshMaterial.color = Color.Lerp(colorStart, colorFeedback, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Hold the end color for 0.5 seconds
        yield return new WaitForSeconds(waitTime);

        // Then transition back to the start color
        elapsedTime = 0;
        while (elapsedTime < duration)
        {
            meshMaterial.color = Color.Lerp(colorFeedback, colorEnd, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        ChangeColor(colorEnd);
    }


    // Plays a sound.
    private void PlaySound(AudioClip audioClip)
    {
        if (!audioSource)
        {
            return;
        }
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    // Plays an animation clip.
    private void PlayAnimation(string animationName)
    {
        // Make sure the mole is in the right state.


        // Play the animation
        playingClip = animationName;
        animationPlayer.PlayQueued(animationName);
    }

    // Returns the duration of the currently playing animation clip.
    private float getAnimationDuration()
    {
        return animationPlayer.GetClip(playingClip).length;
    }

    // Sets up the TransitionColor coroutine to smoothly transition between two colors.
    private void PlayTransitionColor(float duration, Color startColor, Color endColor)
    {
        if (colorAnimation != null) StopCoroutine(colorAnimation);
        colorAnimation = StartCoroutine(TransitionColor(duration, startColor, endColor));
    }

    // Changes the color of the mesh.
    private void ChangeColor(Color color)
    {
        meshMaterial.color = color;
    }

    // Switches between the glowing and standard shader.
    private void SwitchShader(bool glow = false)
    {
        if (glow)
        {
            if (meshMaterial.shader.name == glowShader.name) return;
            meshMaterial.shader = glowShader;
        }
        else
        {
            if (meshMaterial.shader.name == opaqueShader.name) return;
            meshMaterial.shader = opaqueShader;
        }
    }

    // Ease function, Quart ratio.
    private float EaseQuartOut (float k)
    {
        return 1f - ((k -= 1f)*k*k*k);
    }

    private IEnumerator TransitionColor(float duration, Color startColor, Color endColor)
    {
        float durationLeft = duration;
        float totalDuration = duration;

        // Generation of a color gradient from the start color to the end color.
        Gradient colorGradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2]{new GradientColorKey(startColor, 0f), new GradientColorKey(endColor, 1f)};
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2]{new GradientAlphaKey(startColor.a, 0f), new GradientAlphaKey(endColor.a, 1f)};
        colorGradient.SetKeys(colorKey, alphaKey);

        // Playing of the animation. The DiskMole color is interpolated following the easing curve
        while (durationLeft > 0f)
        {
            float timeRatio = (totalDuration - durationLeft) / totalDuration;

            ChangeColor(colorGradient.Evaluate(EaseQuartOut(timeRatio)));
            durationLeft -= Time.deltaTime;

            yield return null;
        }

        // When the animation is finished, resets the color to its end value.
        ChangeColor(endColor);
    }
}
