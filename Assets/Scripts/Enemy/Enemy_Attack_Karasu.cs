﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy_State))]
[RequireComponent(typeof(Enemy_Score))]

public class Enemy_Attack_Karasu : MonoBehaviour {

    [SerializeField]
    TextMesh     Debug_State_Text;      // テスト
    [SerializeField]
    GameObject   m_TargetObj;           // 止まる位置
    [SerializeField]
    GameObject   m_TargetCamera;        // 向くカメラ
    [SerializeField]
    GameObject   m_FadePoint;           // 退却ポイント
    [SerializeField]
    GameObject[] m_LifeList;            // ライフリスト
    [SerializeField]
    float        m_MoveSpeed = 0.05f;   // スピード
    [SerializeField]
    float        m_FadeTimer;           // 退却までのカウント
    [SerializeField]
    SkinnedMeshRenderer m_Color;        // 自分の色
    [SerializeField]
    GameObject   m_EscapeEffect;        // 退却時汗のエフェクト

    private Enemy_State m_State;        // 状態
    private Vector3     m_FadePos;      // 退却座標
    private Color       m_FadeColor;    // 退却時の色
    private Animator    m_Animator;     // アニメション

    // Use this for initialization
    void Start()
    {
        // コンポーネント取得
        m_State    = GetComponent<Enemy_State>();
        m_Animator = GetComponent<Animator>();
        // 変数初期化
        m_FadeColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);      // 現在の色をセット
        // 退却ポイントがない：生成座標を代入
        // 退却ポイントがある：退却ポイント座標を代入
        if( m_FadePoint == null)
        {
            m_FadePos = transform.position;
        }
        else
        {
            m_FadePos = m_FadePoint.transform.position;
        }
        // スコアセット
        Enemy_Score score = GetComponent<Enemy_Score>();
        score.SetScore(Score_List.Enemy.Karasu);
    }

    // Update is called once per frame
    void Update()
    {
        // 状態判定
        switch (m_State.GetState())
        {
            case Enemy_State.STATE.MOVE:     // 移動
                Debug_State_Text.text = "STATE:Move";  // テスト
                // 状態遷移はできない
                m_State.CanSet(false);

                // 対象の位置の方向を向く
                LookAtNoneY(m_TargetObj.transform.position);
                // 目標へ移動
                MoveHoming(m_TargetObj.transform.position, m_MoveSpeed);
                
                // 近い？
                if (Vector3.Distance(transform.position, m_TargetObj.transform.position) <= 1.0f)
                {
                    // 攻撃状態に変更
                    m_State.CanSet(true);
                    m_State.SetState(Enemy_State.STATE.ATTACK);
                    m_Animator.SetBool("MoveToAttack", true);
                }
                break;

            case Enemy_State.STATE.ATTACK:  // ジャマ
                Debug_State_Text.text = "STATE:ジャマジャマ";  // テスト
                // 状態遷移はできない
                m_State.CanSet(false);
                // 目標へ移動
                MoveHoming(m_TargetObj.transform.position, m_MoveSpeed*0.01f);
                LookAtNoneY(m_TargetCamera.transform.position);

                // 退却までのカウント
                m_FadeTimer -= Time.deltaTime;
                if( m_FadeTimer <= 0.0f)
                {
                    // 満足状態に変更
                    m_State.CanSet(true);
                    m_State.SetState(Enemy_State.STATE.SATIETY);
                    m_Animator.SetBool("MoveToAttack", false);
                }
                // 遠い？
                if (Vector3.Distance(transform.position, m_TargetObj.transform.position) > 1.0f)
                {
                    // 移動状態に変更
                    m_State.CanSet(true);
                    m_State.SetState(Enemy_State.STATE.MOVE);
                    m_Animator.SetBool("MoveToAttack", false);
                }
                break;

            case Enemy_State.STATE.SATIETY:  // 満足
                Debug_State_Text.text = "STATE:満足";  // テスト

                // 状態遷移はできない
                m_State.CanSet(false);
                // 離脱の位置の方向に移動
                LookAtNoneY(m_FadePos);
                // 目標へ移動
                MoveHoming(m_FadePos, m_MoveSpeed);

                // 近い？
                if( Vector3.Distance( m_FadePos, transform.position) <= 1.0f)
                {
                    // カラスを消す
                    Destroy(gameObject.transform.parent.gameObject);
                    return;
                }
                break;

            case Enemy_State.STATE.DAMAGE:      // ダメージ状態
                Debug_State_Text.text = "STATE:痛えぇ！";  // テスト

                if( m_State.GetState() == Enemy_State.STATE.MOVE)
                {
                    m_State.SetState(Enemy_State.STATE.MOVE);
                    break;
                }

                m_State.SetState(Enemy_State.STATE.ATTACK);     // 移動状態へ
                break;

            case Enemy_State.STATE.ESCAPE:   // 逃げる
                Debug_State_Text.text = "STATE:FadeOut";  // テスト

                // 状態遷移はもうできない
                m_State.CanSet(false);
                // 汗のエフェクトを出す
                m_EscapeEffect.SetActive(true);
                // 離脱の位置の方向に移動
                LookAtNoneY(m_FadePos);
                // 目標へ移動
                MoveHoming(m_FadePos, m_MoveSpeed);

                // アルファ値を減らす
                m_FadeColor.a -= 0.02f;
                m_Color.material.color = m_FadeColor;

                // 汗を止める
                if (m_FadeColor.a <= 0.3f) { m_EscapeEffect.SetActive(false); }

                // 透明になった親を消す
                if (m_FadeColor.a <= 0.0f) { Destroy(transform.parent.gameObject); }
                return;
        }
        // ライフリストのチェック
        int cnt_life = 0;
        for (int i = 0; i < m_LifeList.Length; ++i)
        {
            if (m_LifeList[i] == null) { cnt_life++; }
            // ライフがなくなったら
            if (cnt_life >= m_LifeList.Length)
            {
                // 透明できる描画モードに変更
                BlendModeUtils.SetBlendMode(m_Color.material, BlendModeUtils.Mode.Fade);
                m_Color.material.color = m_FadeColor;
                m_State.CanSet(true);
                m_State.SetState(Enemy_State.STATE.ESCAPE);     // 離脱状態へ
            }
        }
    }

    // Y軸無視でターゲットに向く
    void LookAtNoneY(Vector3 TargetPos)
    {
        TargetPos.y = transform.position.y;       // y軸無視
        transform.LookAt(TargetPos);              // ターゲットに向く
        transform.Rotate(new Vector3(0, 60, 0));  // 向きの微調整
    }

    // 目標に移動
    void MoveHoming(Vector3 TargetPos, float Speed)
    {
        Vector3 move = TargetPos - transform.position;   // 目的へのベクトル
        move = move.normalized;                          // 正規化
        transform.position += move * Speed;              // 移動処理
    }
}
