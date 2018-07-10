using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Footsies
{
    /// <summary>
    /// Compute the fight area that is going to be on screen and update fighter sprites position
    /// Also update the debug display
    /// </summary>
    public class BattleGUI : MonoBehaviour
    {
        #region serialize field

        [SerializeField]
        private GameObject _battleCoreGameObject;

        [SerializeField]
        private GameObject fighter1ImageObject;

        [SerializeField]
        private GameObject fighter2ImageObject;

        [SerializeField]
        private GameObject hitEffectObject1;

        [SerializeField]
        private GameObject hitEffectObject2;

        [SerializeField]
        private float _battleBoxLineWidth = 2f;

        [SerializeField]
        private GUIStyle debugTextStyle;

        [SerializeField]
        private bool drawDebug = false;

        #endregion

        #region private field

        private BattleCore battleCore;

        private Vector2 battleAreaTopLeftPoint;
        private Vector2 battleAreaBottomRightPoint;

        private Vector2 fightPointToScreenScale;
        private float centerPoint;

        private RectTransform rectTransform;

        private Image fighter1Image;
        private Image fighter2Image;

        private Animator hitEffectAnimator1;
        private Animator hitEffectAnimator2;

        #endregion

        private void Awake()
        {
            rectTransform = gameObject.GetComponent<RectTransform>();

            if (_battleCoreGameObject != null)
            {
                battleCore = _battleCoreGameObject.GetComponent<BattleCore>();
                battleCore.damageHandler += OnDamageHandler;
            }

            if (fighter1ImageObject != null)
                fighter1Image = fighter1ImageObject.GetComponent<Image>();
            if (fighter2ImageObject != null)
                fighter2Image = fighter2ImageObject.GetComponent<Image>();

            if (hitEffectObject1 != null)
                hitEffectAnimator1 = hitEffectObject1.GetComponent<Animator>();
            if (hitEffectObject2 != null)
                hitEffectAnimator2 = hitEffectObject2.GetComponent<Animator>();
        }

        private void OnDestroy()
        {
            battleCore.damageHandler -= OnDamageHandler;
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                drawDebug = !drawDebug;
            }

            CalculateBattleArea();
            CalculateFightPointToScreenScale();

            UpdateSprite();
        }

        void OnGUI()
        {
            if (drawDebug)
            {
                battleCore.fighters.ForEach((f) => DrawFighter(f));
                
                var labelRect = new Rect(Screen.width * 0.4f, Screen.height * 0.95f, Screen.width * 0.2f, Screen.height * 0.05f);
                debugTextStyle.alignment = TextAnchor.UpperCenter;
                GUI.Label(labelRect, "F1=Pause/Resume, F2=Frame Step, F12=Debug Draw", debugTextStyle);

                //DrawBox(new Rect(battleAreaTopLeftPoint.x,
                //    battleAreaTopLeftPoint.y,
                //    battleAreaBottomRightPoint.x - battleAreaTopLeftPoint.x,
                //    battleAreaBottomRightPoint.y - battleAreaTopLeftPoint.y),
                //    Color.gray, true);
            }
        }

        void UpdateSprite()
        {
            if(fighter1Image != null)
            {
                var sprite = battleCore.fighter1.GetCurrentMotionSprite();
                if(sprite != null)
                    fighter1Image.sprite = sprite;
                
                var position = fighter1Image.transform.position;
                position.x = TransformHorizontalFightPointToScreen(battleCore.fighter1.position.x) + battleCore.fighter1.spriteShakePosition;
                fighter1Image.transform.position = position;
            }

            if (fighter2Image != null)
            {
                var sprite = battleCore.fighter2.GetCurrentMotionSprite();
                if (sprite != null)
                    fighter2Image.sprite = sprite;

                var position = fighter2Image.transform.position;
                position.x = TransformHorizontalFightPointToScreen(battleCore.fighter2.position.x) + battleCore.fighter2.spriteShakePosition;
                fighter2Image.transform.position = position;
                
            }
        }

        void DrawFighter(Fighter fighter)
        {
            var labelRect = new Rect(0, Screen.height * 0.86f, Screen.width * 0.22f, 50);
            if (fighter.isFaceRight)
            {
                labelRect.x = Screen.width * 0.01f;
                debugTextStyle.alignment = TextAnchor.UpperLeft;
            }
            else
            {
                labelRect.x = Screen.width * 0.77f;
                debugTextStyle.alignment = TextAnchor.UpperRight;
            }

            GUI.Label(labelRect, fighter.position.ToString(), debugTextStyle);

            labelRect.y += Screen.height * 0.03f;
            var frameAdvantage = battleCore.GetFrameAdvantage(fighter.isFaceRight);
            var frameAdvText = frameAdvantage > 0 ? "+" + frameAdvantage : frameAdvantage.ToString();
            GUI.Label(labelRect, "Frame: " + fighter.currentActionFrame + "/" + fighter.currentActionFrameCount 
                + "(" + frameAdvText + ")", debugTextStyle);

            labelRect.y += Screen.height * 0.03f;
            GUI.Label(labelRect, "Stun: " + fighter.currentHitStunFrame, debugTextStyle);

            labelRect.y += Screen.height * 0.03f;
            GUI.Label(labelRect, "Action: " + fighter.currentActionID + " " + (CommonActionID)fighter.currentActionID, debugTextStyle);

            foreach (var hurtbox in fighter.hurtboxes)
            {
                DrawFightBox(hurtbox.rect, Color.yellow, true);
            }

            if (fighter.pushbox != null)
            {
                DrawFightBox(fighter.pushbox.rect, Color.blue, true);
            }

            foreach (var hitbox in fighter.hitboxes)
            {
                if(hitbox.proximity)
                    DrawFightBox(hitbox.rect, Color.gray, true);
                else
                    DrawFightBox(hitbox.rect, Color.red, true);
            }
        }

        void DrawFightBox(Rect fightPointRect, Color color, bool isFilled)
        {
            var screenRect = new Rect();
            screenRect.width = fightPointRect.width * fightPointToScreenScale.x;
            screenRect.height = fightPointRect.height * fightPointToScreenScale.y;
            screenRect.x = TransformHorizontalFightPointToScreen(fightPointRect.x) - (screenRect.width / 2);
            screenRect.y = battleAreaBottomRightPoint.y - (fightPointRect.y * fightPointToScreenScale.y) - screenRect.height;

            DrawBox(screenRect, color, isFilled);
        }

        void DrawBox(Rect rect, Color color, bool isFilled)
        {
            float startX = rect.x;
            float startY = rect.y;
            float width = rect.width;
            float height = rect.height;
            float endX = startX + width;
            float endY = startY + height;

            Draw.DrawLine(new Vector2(startX, startY), new Vector2(endX, startY), color, _battleBoxLineWidth);
            Draw.DrawLine(new Vector2(startX, startY), new Vector2(startX, endY), color, _battleBoxLineWidth);
            Draw.DrawLine(new Vector2(endX, endY), new Vector2(endX, startY), color, _battleBoxLineWidth);
            Draw.DrawLine(new Vector2(endX, endY), new Vector2(startX, endY), color, _battleBoxLineWidth);

            if (isFilled)
            {
                Color rectColor = color;
                rectColor.a = 0.25f;
                Draw.DrawRect(new Rect(startX, startY, width, height), rectColor);
            }
        }

        float TransformHorizontalFightPointToScreen(float x)
        {
            return (x * fightPointToScreenScale.x) + centerPoint;
        }

        float TransformVerticalFightPointToScreen(float y)
        {
            return (Screen.height - battleAreaBottomRightPoint.y) + (y * fightPointToScreenScale.y);
        }

        void CalculateBattleArea()
        {
            Vector3[] v = new Vector3[4];
            rectTransform.GetWorldCorners(v);
            battleAreaTopLeftPoint = new Vector2(v[1].x, Screen.height - v[1].y);
            battleAreaBottomRightPoint = new Vector2(v[3].x, Screen.height - v[3].y);
        }

        void CalculateFightPointToScreenScale()
        {
            fightPointToScreenScale.x = (battleAreaBottomRightPoint.x - battleAreaTopLeftPoint.x) / battleCore.battleAreaWidth;
            fightPointToScreenScale.y = (battleAreaBottomRightPoint.y - battleAreaTopLeftPoint.y) / battleCore.battleAreaMaxHeight;

            centerPoint = (battleAreaBottomRightPoint.x + battleAreaTopLeftPoint.x) / 2;
        }

        private void OnDamageHandler(Fighter damagedFighter, Vector2 damagedPos, DamageResult damageResult)
        {
            // Set attacker fighter to last sibling so that it will get draw last and be on the most front
            if(damagedFighter == battleCore.fighter1)
            {
                fighter2Image.transform.SetAsLastSibling();

                RequestHitEffect(hitEffectAnimator1, damagedPos, damageResult);
            }
            else if(damagedFighter == battleCore.fighter2)
            {
                fighter1Image.transform.SetAsLastSibling();

                RequestHitEffect(hitEffectAnimator2, damagedPos, damageResult);
            }
        }

        private void RequestHitEffect(Animator hitEffectAnimator, Vector2 damagedPos, DamageResult damageResult)
        {
            hitEffectAnimator.SetTrigger("Hit");
            var position = hitEffectAnimator2.transform.position;
            position.x = TransformHorizontalFightPointToScreen(damagedPos.x);
            position.y = TransformVerticalFightPointToScreen(damagedPos.y);
            hitEffectAnimator.transform.position = position;

            if (damageResult == DamageResult.GuardBreak)
                hitEffectAnimator.transform.localScale = new Vector3(5,5,1);
            else if (damageResult == DamageResult.Damage)
                hitEffectAnimator.transform.localScale = new Vector3(2, 2, 1);
            else if (damageResult == DamageResult.Guard)
                hitEffectAnimator.transform.localScale = new Vector3(1, 1, 1);

            hitEffectAnimator.transform.SetAsLastSibling();
        }
    }

}