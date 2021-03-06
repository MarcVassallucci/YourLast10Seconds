﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class EyeClosing : MonoBehaviour
{
    [SerializeField] float _totalTime = 10f;
    [SerializeField] public float _fadeSpeed = 7f;
    [SerializeField] public float _blinkSpeed = 30f;
    [SerializeField] public float _blinkDuration = .1f;
    [SerializeField] float _eyeShakeAmplitude = 0.0025f;
    [SerializeField] EnergyBar _energyBar = null;
    [SerializeField] Game _game = null;
    [SerializeField] Image _blackOverlay = null;
    [SerializeField] SkinnedMeshRenderer _eye = null;
    [SerializeField] AudioMixer _mixer = null;

    [HideInInspector] public bool _paused = true;

    float _usedTime = 0;
    bool _forcedShut = false;

    void Update()
    {
        bool ShouldBeClosed = _game.State != GameState.Scene
            || Input.GetKey(KeyCode.Space) == false
            || _forcedShut;

        _blackOverlay.color = Color.Lerp(_blackOverlay.color,
            ShouldBeClosed ? Color.black : new Color(0f, 0f, 0f, 0f),
            Time.deltaTime * _fadeSpeed);

        _eye.SetBlendShapeWeight(0, Mathf.Lerp(_eye.GetBlendShapeWeight(0),
            ShouldBeClosed ? 100f : 0f,
            Time.deltaTime * _fadeSpeed));

        _eye.transform.localPosition = new Vector3(
            Random.Range(-_eyeShakeAmplitude, _eyeShakeAmplitude),
            Random.Range(-_eyeShakeAmplitude, _eyeShakeAmplitude),
            _eye.transform.localPosition.z);

        _eye.transform.localScale = Vector3.one * Mathf.Lerp(_eye.transform.localScale.x, ShouldBeClosed ? .7f : 1f, Time.deltaTime * _fadeSpeed);

        if (ShouldBeClosed)
            _mixer.FindSnapshot("Muffled").TransitionTo(.2f);
        else
            _mixer.FindSnapshot("Normal").TransitionTo(.5f);

        _energyBar.SetIsInDanger(!ShouldBeClosed);

        if (_game.State == GameState.Scene && Input.GetKey(KeyCode.Space) && _paused == false)
        {
            _usedTime += Time.deltaTime;
            _energyBar.SetRatio(1f - (_usedTime / _totalTime));

            if (_usedTime >= _totalTime)
            {
                _game.LoadFinalScene();
            }
        }
    }
    
    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(CloseInternal());
    }

    IEnumerator CloseInternal()
    {
        float InitialFadeSpeed = _fadeSpeed;

        _forcedShut = true;
        _fadeSpeed = _blinkSpeed;

        yield return new WaitForSeconds(_blinkDuration * .5f);

        _forcedShut = false;

        yield return new WaitForSeconds(_blinkDuration *.5f);

        _fadeSpeed = InitialFadeSpeed;
    }
}
