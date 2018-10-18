﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Life))]
[RequireComponent(typeof(Enemy_State))]
[RequireComponent(typeof(Enemy_Score))]

public class Enemy_Karasu : MonoBehaviour {

    [SerializeField]
    TextMesh Debug_State_Text;
    [SerializeField]
    Renderer m_Color;               // 自分の色
    [SerializeField]
    GameObject[] m_NavObj;          // 目標オブジェクト
    [SerializeField]
    float m_Speed = 0.05f;          // スピード
    [SerializeField]
    float m_EatSpeed = 1.0f;        // 食べるスピード

    private Enemy_State m_State;    // 状態
    private GameObject m_TargetObj; // ターゲット
    private Life m_Life;            // 体力
    private Vector3 m_PosOld;       // 生成座標

    // Use this for initialization
    void Start()
    {
        // ターゲットの代入
        m_TargetObj = m_NavObj[0];
        // モードの取得
        m_State = GetComponent<Enemy_State>();
        // 体力の取得
        m_Life = GetComponent<Life>();
        // スコアセット
        Enemy_Score score = GetComponent<Enemy_Score>();
        score.SetScore(Score_List.Enemy.Karasu);

        // 生成座標の記憶
        m_PosOld = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 状態判定
        switch (m_State.GetState())
        {
            case Enemy_State.STATE.NORMAL:   // 通常
                Debug_State_Text.text = "STATE:Normal";
                m_State.SetState(Enemy_State.STATE.MOVE);
                break;

            case Enemy_State.STATE.MOVE:     // 移動
                Debug_State_Text.text = "STATE:Move";
                // 目標がなくなった？
                if (m_TargetObj == null)
                {
                    // 再検索
                    m_TargetObj = SerchCrops();          // 農作物をサーチ
                    break;
                }
                //対象の位置の方向を向く
                LookAtNoneY(m_TargetObj);
                // 目標へ移動
                MoveHoming(m_TargetObj, m_Speed);

                // 目標が農作物？
                if( m_TargetObj.tag != "Crops") { break; }
                // 近い？
                if (Vector3.Distance(transform.position, m_TargetObj.transform.position) <= 1.0f)
                {
                    // 食べる状態に変更
                    m_State.SetState(Enemy_State.STATE.EAT);
                }
                break;

            case Enemy_State.STATE.EAT:      // 食べる
                Debug_State_Text.text = "STATE:食べているよ";
                if (m_TargetObj == null) { break; }

                // 農作物のライフを取得
                Life target_life = m_TargetObj.GetComponent<Life>();
                // ライフを削る
                target_life.SubLife(Time.deltaTime * m_EatSpeed);
                // 食べ終わった？
                if ( target_life.GetLife() <= 0)
                {
                    // 農作物を消す
                    Destroy(m_TargetObj.gameObject);
                    // 満腹になる
                    m_State.SetState(Enemy_State.STATE.SATIETY);
                    // 退却座標の代入
                    m_TargetObj = m_NavObj[1];
                }
                break;

            case Enemy_State.STATE.SATIETY:  // 満腹
                Debug_State_Text.text = "STATE:満腹";
                // 目標に着いた？
                if (m_TargetObj == null)
                {
                    // カラスを消す
                    Destroy(gameObject.transform.parent.gameObject);
                    return;
                }

                // 対象の位置の方向を向く
                LookAtNoneY(m_TargetObj);
                // 目標へ移動
                MoveHoming(m_TargetObj, m_Speed * 0.5f);
                break;

            case Enemy_State.STATE.DAMAGE:      // ダメージ状態
                // 体力を減らす
                m_Life.SubLife(1.0f);

                // 体力がなくなった？
                if (m_Life.GetLife() <= 0)
                {
                    m_State.SetState(Enemy_State.STATE.ESCAPE);     // 離脱状態へ
                }
                break;

            case Enemy_State.STATE.ESCAPE:   // 逃げる
                Debug_State_Text.text = "STATE:FadeOut";

                // 離脱の位置の方向に移動
                LookAtNoneY(m_PosOld);
                // 目標へ移動
                MoveHoming(m_PosOld, m_Speed);

                // アルファ値を減らす
                Color color = m_Color.material.color;
                color.a -= 0.01f;
                m_Color.material.color = color;

                // 透明になった？
                if (color.a > 0.0f) { break; }

                // カラスを消す
                Destroy(gameObject.transform.parent.gameObject);
                return;
        }
    }

    // リストから目標を取得
    GameObject SerchCrops()
    {
        //目標を配列で取得する
        foreach (GameObject obs in m_NavObj)
        {
            if (obs == null) continue;
            if( obs.tag == "Crops")
            {
                return obs;
            }
        }
        // 満腹になる
        m_State.SetState(Enemy_State.STATE.SATIETY);
        // 退却座標の代入
        m_TargetObj = m_NavObj[1];
        // リストがなくなったらnull
        return null;
    }

    // Y軸無視でターゲットに向く
    void LookAtNoneY(GameObject Target)
    {
        Vector3 target = Target.transform.position;
        target.y = transform.position.y;       // y軸無視
        transform.LookAt(target);
    }
    void LookAtNoneY(Vector3 TargetPos)
    {
        TargetPos.y = transform.position.y;       // y軸無視
        transform.LookAt(TargetPos);
    }

    // 目標に移動
    void MoveHoming(GameObject Target, float Speed)
    {
        Vector3 move = Target.transform.position - transform.position;   // 目的へのベクトル
        move = move.normalized;                                             // 正規化
        transform.position += move * m_Speed;
    }
    void MoveHoming(Vector3 TargetPos, float Speed)
    {
        Vector3 move = TargetPos - transform.position;   // 目的へのベクトル
        move = move.normalized;                                             // 正規化
        transform.position += move * m_Speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Point")
        {
            Destroy(other.gameObject);
        }
    }
}
