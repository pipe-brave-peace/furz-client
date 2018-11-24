﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Operation2_Enemy : MonoBehaviour
{

    [SerializeField]
    RectTransform rect;

    [SerializeField]
    float StartRot;

    [SerializeField]
    float EndRot;

    [SerializeField]
    GameObject Damege;

    [SerializeField]
    bool m_bUse;

    [SerializeField]
    GameObject SmokeOn_UI;

    [SerializeField]
    Sprite[] m_Sprite;

    [SerializeField]
    Image m_Image;

    public bool m_bSmokeOn;

    public bool m_bBusterOn;

    Vector3 OldPos;

    // Use this for initialization
    void Start()
    {
        m_Image.sprite = m_Sprite[0];
        OldPos = rect.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

        if (!m_bUse)
        {
            if (StartRot < rect.rotation.z)
            {
                rect.rotation = new Quaternion(rect.rotation.x, rect.rotation.y, rect.rotation.z - 0.01f, rect.rotation.w);
            }
            else if (StartRot >= rect.rotation.z)
            {
                m_bUse = true;
            }
        }
        else
        {
            if (EndRot > rect.rotation.z)
            {
                rect.rotation = new Quaternion(rect.rotation.x, rect.rotation.y, rect.rotation.z + 0.01f, rect.rotation.w);
            }
            else if (EndRot <= rect.rotation.z)
            {
                m_bUse = false;
            }
        }

        if (m_bSmokeOn)
        {
            SmokeOn_UI.SetActive(true);
            if (m_bBusterOn)
            {
                m_Image.sprite = m_Sprite[1];
                
                if (rect.localPosition.y > -300)
                {
                    rect.localPosition -= new Vector3(0, 6, 0);
                }
                else if (rect.localPosition.y <= -300)
                {
                    m_Image.sprite = m_Sprite[0];
                    Damege.SetActive(false);
                    SmokeOn_UI.SetActive(false);
                    m_bBusterOn = false;
                    m_bSmokeOn = false;
                    rect.localPosition = OldPos;
                    rect.rotation = new Quaternion(rect.rotation.x, 0, 0, rect.rotation.w);
                }
            }
        }

    }
}
