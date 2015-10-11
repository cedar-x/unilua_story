using UnityEngine;
using System.Collections;
using Hstj;

public class EffectDelay : MonoBehaviour 
{
    public float m_fDelay;
    private Animator[] m_animators = null;
    private ParticleSystem[] m_syses = null;
    private LuaEffect m_pEffect = null;
    private bool m_bStarted = false;

	void Awake () 
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            anim.enabled = false;
        }
	}
	    
	void Update () 
    {
        if (m_pEffect == null || !m_pEffect.Running)
            return;
		
		float fInterval = Time.time - m_pEffect.StartTime;

        if (fInterval >= (m_fDelay + m_pEffect.StartDelay) && m_bStarted == false)
        {
            m_bStarted = true;
            DoPlayEffect();
        }
	}

    public void ReInit(LuaEffect pEffect)
    {
        m_pEffect = pEffect;

        m_animators = GetComponentsInChildren<Animator>();

        m_syses = GetComponentsInChildren<ParticleSystem>();

    }

    public void InitMember(LuaEffect pEffect)
    {
        m_pEffect = pEffect;

        if (m_animators == null)
        {
            m_animators = GetComponentsInChildren<Animator>();
        }

        if (m_syses == null)
        {
            m_syses = GetComponentsInChildren<ParticleSystem>();
        }
    }

    public void Play()
    {
        m_bStarted = false;
    }

    public void Stop()
    {
        for (int i = 0; i < m_syses.Length; ++i)
        {
            m_syses[i].gameObject.SetActive(true);
            m_syses[i].Play();
        }

        for (int i = 0; i < m_animators.Length; ++i)
        {
            m_animators[i].Rebind();
            m_animators[i].Play(m_animators[i].GetCurrentAnimatorStateInfo(0).nameHash);
        }
    }

    private void DoPlayEffect()
    {
        for (int i = 0; i < m_syses.Length; ++i)
        {
            m_syses[i].gameObject.SetActive(true);
            m_syses[i].Play();
        }

        for (int i = 0; i < m_animators.Length; ++i)
        {
            m_animators[i].enabled = true;
            m_animators[i].Rebind();
            m_animators[i].Play(m_animators[i].GetCurrentAnimatorStateInfo(0).nameHash);
        }

    }

}
