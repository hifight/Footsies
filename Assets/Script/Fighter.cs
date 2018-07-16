using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Footsies
{

    public class BoxBase
    {
        public Rect rect;

        public float xMin { get { return rect.x - rect.width / 2; } }
        public float xMax { get { return rect.x + rect.width / 2; } }
        public float yMin { get { return rect.y; } }
        public float yMax { get { return rect.y + rect.height; } }

        public bool Overlaps(BoxBase otherBox)
        {
            var c1 = otherBox.xMax >= xMin;
            var c2 = otherBox.xMin <= xMax;
            var c3 = otherBox.yMax >= yMin;
            var c4 = otherBox.yMin <= yMax;

            return c1 && c2 && c3 && c4;
        }
    }

    public class Hitbox : BoxBase
    {
        public bool proximity = false;
        public int attackID;
    }

    public class Hurtbox : BoxBase
    {
    }

    public class Pushbox : BoxBase
    {
    }

    public enum CommonActionID
    {
        STAND = 0,
        FORWARD = 1,
        BACKWARD = 2,
        DASH_FORWARD = 10,
        DASH_BACKWARD = 11,
        N_ATTACK = 100,
        B_ATTACK = 105,
        N_SPECIAL = 110,
        B_SPECIAL = 115,
        DAMAGE = 200,
        GUARD_M = 301,
        GUARD_STAND = 305,
        GUARD_CROUCH = 306,
        GUARD_BREAK = 310,
        GUARD_PROXIMITY = 350,
        DEAD = 500,
        WIN = 510,
    }

    public enum DamageResult
    {
        Damage = 1,
        Guard ,
        GuardBreak,
        Counter,
    }
    
    public class Fighter
    {
        public Vector2 position;
        public float velocity_x;
        public bool isFaceRight;

        public List<Hitbox> hitboxes = new List<Hitbox>();
        public List<Hurtbox> hurtboxes = new List<Hurtbox>();
        public Pushbox pushbox;

        private FighterData fighterData;

        public bool isDead { get { return vitalHealth <= 0; } }
        public int vitalHealth { get; private set; }
        public int guardHealth { get; private set; }

        public int currentActionID { get; private set; }
        public int currentActionFrame { get; private set; }
        public int currentActionFrameCount { get { return fighterData.actions[currentActionID].frameCount; } }
        private bool isActionEnd { get { return (currentActionFrame >= fighterData.actions[currentActionID].frameCount); } }
        public bool isAlwaysCancelable { get { return fighterData.actions[currentActionID].alwaysCancelable; } }
        
        public int currentActionHitCount{ get; private set; }

        public int currentHitStunFrame { get; private set; }
        public bool isInHitStun { get { return currentHitStunFrame > 0; } }

        private static int inputRecordFrame = 180;
        private int[] input = new int[inputRecordFrame];
        private int[] inputDown = new int[inputRecordFrame];
        private int[] inputUp = new int[inputRecordFrame];

        private bool isInputBackward;
        private bool isReserveProximityGuard;

        private int bufferActionID = -1;
        private int reserveDamageActionID = -1;

        public int spriteShakePosition { get; private set; }
        private int maxSpriteShakeFrame = 6;

        private bool hasWon = false;

        /// <summary>
        /// Setup fighter at the start of battle
        /// </summary>
        /// <param name="fighterData"></param>
        /// <param name="startPosition"></param>
        /// <param name="isPlayerOne"></param>
        public void SetupBattleStart(FighterData fighterData, Vector2 startPosition, bool isPlayerOne)
        {
            this.fighterData = fighterData;
            position = startPosition;
            isFaceRight = isPlayerOne;

            vitalHealth = 1;
            guardHealth = fighterData.startGuardHealth;
            hasWon = false;

            velocity_x = 0;

            ClearInput();

            SetCurrentAction((int)CommonActionID.STAND);
        }
        
        /// <summary>
        /// Update action frame
        /// </summary>
        public void IncrementActionFrame()
        {
            // Decrease sprite shake count and swap +/- (used by BattleGUI for fighter sprite position)
            if (Mathf.Abs(spriteShakePosition) > 0)
            {
                spriteShakePosition *= -1;
                spriteShakePosition += (spriteShakePosition > 0 ? -1 : 1);
            }

            // If fighter is in hit stun then the action frame stay the same
            if (currentHitStunFrame > 0)
            {
                currentHitStunFrame--;
                return;
            }

            currentActionFrame++;

            // For loop motion (winning pose etc.) set action frame back to loop start frame
            if (isActionEnd)
            {
                if(fighterData.actions[currentActionID].isLoop)
                {
                    currentActionFrame = fighterData.actions[currentActionID].loopFromFrame;
                }
            }
        }

        /// <summary>
        /// UpdateInput
        /// </summary>
        /// <param name="inputData"></param>
        public void UpdateInput(InputData inputData)
        {
            // Shift input history by 1 frame
            for(int i = input.Length - 1; i >= 1; i--)
            {
                input[i] = input[i - 1];
                inputDown[i] = inputDown[i - 1];
                inputUp[i] = inputUp[i - 1];
            }

            // Insert new input data
            input[0] = inputData.input;
            inputDown[0] = (input[0] ^ input[1]) & input[0];
            inputUp[0] = (input[0] ^ input[1]) & ~input[0];
            //Debug.Log(System.Convert.ToString(input[0], 2) + " " + System.Convert.ToString(inputDown[0], 2) + " " + System.Convert.ToString(inputUp[0], 2));

        }

        /// <summary>
        /// Action request for intro state ()
        /// </summary>
        public void UpdateIntroAction()
        {
            RequestAction((int)CommonActionID.STAND);
        }

        /// <summary>
        /// Update action request
        /// </summary>
        public void UpdateActionRequest()
        {
            // If won then just request win animation
            if(hasWon)
            {
                RequestAction((int)CommonActionID.WIN);
                return;
            }

            // If there is any reserve damage action, set that to current action
            // Use for playing damage motion after hit stun ended (only use this for guard break currently)
            if (reserveDamageActionID != -1
                && currentHitStunFrame <= 0)
            {
                SetCurrentAction(reserveDamageActionID);
                reserveDamageActionID = -1;
                return;
            }

            // If there is any buffer action, set that to current action
            // Use for canceling normal to special attack
            if (bufferActionID != -1 
                && canCancelAttack()
                && currentHitStunFrame <= 0)
            {
                SetCurrentAction(bufferActionID);
                bufferActionID = -1;
                return;
            }

            var isForward = IsForwardInput(input[0]);
            var isBackward = IsBackwardInput(input[0]);
            bool isAttack = IsAttackInput(inputDown[0]);
            if (CheckSpecialAttackInput())
            {
                if (isBackward || isForward)
                    RequestAction((int)CommonActionID.B_SPECIAL);
                else
                    RequestAction((int)CommonActionID.N_SPECIAL);
            }
            else if (isAttack)
            {
                if ((currentActionID == (int)CommonActionID.N_ATTACK ||
                    currentActionID == (int)CommonActionID.B_ATTACK) &&
                    !isActionEnd)
                    RequestAction((int)CommonActionID.N_SPECIAL);
                else
                {
                    if(isBackward || isForward)
                        RequestAction((int)CommonActionID.B_ATTACK);
                    else
                        RequestAction((int)CommonActionID.N_ATTACK);
                }
            }

            if(CheckForwardDashInput())
                RequestAction((int)CommonActionID.DASH_FORWARD);
            else if (CheckBackwardDashInput())
                RequestAction((int)CommonActionID.DASH_BACKWARD);


            // for proximity guard check
            isInputBackward = isBackward;

            if (isForward && isBackward)
            {
                RequestAction((int)CommonActionID.STAND);
            }
            else if (isForward)
            {
                RequestAction((int)CommonActionID.FORWARD);
            }
            else if (isBackward)
            {
                if(isReserveProximityGuard)
                    RequestAction((int)CommonActionID.GUARD_PROXIMITY);
                else
                    RequestAction((int)CommonActionID.BACKWARD);
            }
            else
            {
                RequestAction((int)CommonActionID.STAND);
            }

            isReserveProximityGuard = false;
        }

        /// <summary>
        /// Update character position
        /// </summary>
        public void UpdateMovement()
        {
            if (isInHitStun)
                return;

            // Position changes from walking forward and backward
            var sign = isFaceRight ? 1 : -1;
            if (currentActionID == (int)CommonActionID.FORWARD)
            {
                position.x += fighterData.forwardMoveSpeed * sign * Time.deltaTime;
                return;
            }
            else if (currentActionID == (int)CommonActionID.BACKWARD)
            {
                position.x -= fighterData.backwardMoveSpeed * sign * Time.deltaTime;
                return;
            }

            // Position changes from action data
            var movementData = fighterData.actions[currentActionID].GetMovementData(currentActionFrame);
            if (movementData != null)
            {
                velocity_x = movementData.velocity_x;
                if (velocity_x != 0)
                {
                    position.x += velocity_x * sign * Time.deltaTime;
                }
            }
        }

        public void UpdateBoxes()
        {
            ApplyCurrentActionData();
        }

        /// <summary>
        /// Apply position changed to all variable
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ApplyPositionChange(float x, float y)
        {
            position.x += x;
            position.y += y;

            foreach (var hitbox in hitboxes)
            {
                hitbox.rect.x += x;
                hitbox.rect.y += y;
            }

            foreach (var hurtbox in hurtboxes)
            {
                hurtbox.rect.x += x;
                hurtbox.rect.y += y;
            }

            pushbox.rect.x += x;
            pushbox.rect.y += y;
        }

        public void NotifyAttackHit(Fighter damagedFighter, Vector2 damagePos)
        {
            currentActionHitCount++;
        }

        public DamageResult NotifyDamaged(AttackData attackData, Vector2 damagePos)
        {
            bool isGuardBreak = false;
            if (attackData.guardHealthDamage > 0)
            {
                guardHealth -= attackData.guardHealthDamage;
                if (guardHealth < 0)
                {
                    isGuardBreak = true;
                    guardHealth = 0;
                }
            }

            if (currentActionID == (int)CommonActionID.BACKWARD
                || fighterData.actions[currentActionID].Type == ActionType.Guard) // if in blocking motion, automatically block next attack
            {
                if (isGuardBreak)
                {
                    SetCurrentAction(attackData.guardActionID);
                    reserveDamageActionID = (int)CommonActionID.GUARD_BREAK;
                    SoundManager.Instance.playFighterSE(fighterData.actions[reserveDamageActionID].audioClip, isFaceRight, position.x);
                    return DamageResult.GuardBreak;
                }
                else
                {
                    SetCurrentAction(attackData.guardActionID);
                    return DamageResult.Guard;
                }
            }
            else
            {
                if(attackData.vitalHealthDamage > 0)
                {
                    vitalHealth -= attackData.vitalHealthDamage;
                    if (vitalHealth <= 0)
                        vitalHealth = 0;
                }
                
                SetCurrentAction(attackData.damageActionID);
                return DamageResult.Damage;
            }
        }

        public void NotifyInProximityGuardRange()
        {
            if(isInputBackward)
            {
                isReserveProximityGuard = true;
            }
        }

        public bool CanAttackHit(int attackID)
        {
            if(!fighterData.attackData.ContainsKey(attackID))
            {
                Debug.LogWarning("Attack hit but AttackID=" + attackID + " is not registered");
                return true;
            }

            if (currentActionHitCount >= fighterData.attackData[attackID].numberOfHit)
                return false;

            return true;
        }

        public AttackData getAttackData(int attackID)
        {
            if (!fighterData.attackData.ContainsKey(attackID))
            {
                Debug.LogWarning("Attack hit but AttackID=" + attackID + " is not registered");
                return null;
            }
            
            return fighterData.attackData[attackID];
        }

        public void SetHitStun(int hitStunFrame)
        {
            currentHitStunFrame = hitStunFrame;
        }

        public void SetSpriteShakeFrame(int spriteShakeFrame)
        {
            if (spriteShakeFrame > maxSpriteShakeFrame)
                spriteShakeFrame = maxSpriteShakeFrame;

            spriteShakePosition = spriteShakeFrame * (isFaceRight ? -1 : 1);
        }

        public int GetHitStunFrame(DamageResult damageResult, int attackID)
        {
            if(damageResult == DamageResult.Guard)
                return fighterData.attackData[attackID].guardStunFrame;
            else if (damageResult == DamageResult.GuardBreak)
                return fighterData.attackData[attackID].guardBreakStunFrame;

            return fighterData.attackData[attackID].hitStunFrame;
        }

        public int getGuardStunFrame(int attackID)
        {
            return fighterData.attackData[attackID].guardStunFrame;
        }

        public void RequestWinAction()
        {
            hasWon = true;
        }

        /// <summary>
        /// Request action, if condition is met then set the requested action to current action
        /// </summary>
        /// <param name="actionID"></param>
        /// <param name="startFrame"></param>
        /// <returns></returns>
        public bool RequestAction(int actionID, int startFrame = 0)
        {
            if (isActionEnd)
            {
                SetCurrentAction(actionID, startFrame);
                return true;
            }

            if (currentActionID == actionID)
            {
                return false;
            }

            if (fighterData.actions[currentActionID].alwaysCancelable)
            {
                SetCurrentAction(actionID, startFrame);
                return true;
            }
            else
            {
                foreach (var cancelData in fighterData.actions[currentActionID].GetCancelData(currentActionFrame))
                {
                    if (cancelData.actionID.Contains(actionID))
                    {
                        if (cancelData.execute)
                        {
                            bufferActionID = actionID;
                            return true;
                        }
                        else if (cancelData.buffer)
                        {
                            bufferActionID = actionID;
                        }
                    }
                }
            }

            return false;
        }

        public Sprite GetCurrentMotionSprite()
        {
            var motionData = fighterData.actions[currentActionID].GetMotionData(currentActionFrame);
            if (motionData == null)
                return null;

            return fighterData.motionData[motionData.motionID].sprite;
        }

        public void ClearInput()
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = 0;
                inputDown[i] = 0;
                inputUp[i] = 0;
            }
        }
        
        private bool canCancelAttack()
        {
            if (fighterData.canCancelOnWhiff)
                return true;
            else if (currentActionHitCount > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Set current action
        /// </summary>
        /// <param name="actionID"></param>
        /// <param name="startFrame"></param>
        private void SetCurrentAction(int actionID, int startFrame = 0)
        {
            currentActionID = actionID;
            currentActionFrame = startFrame;

            currentActionHitCount = 0;
            bufferActionID = -1;
            reserveDamageActionID = -1;
            spriteShakePosition = 0;

            if(fighterData.actions[currentActionID].audioClip != null)
            {
                if (currentActionID == (int)CommonActionID.GUARD_BREAK)
                    return;

                SoundManager.Instance.playFighterSE(fighterData.actions[currentActionID].audioClip, isFaceRight, position.x);
            }
        }
        
        /// <summary>
        /// Special attack input check (hold and release)
        /// </summary>
        /// <returns></returns>
        private bool CheckSpecialAttackInput()
        {
            if (!IsAttackInput(inputUp[0]))
                return false;

            for (int i = 1; i < fighterData.specialAttackHoldFrame; i++)
            {
                if (!IsAttackInput(input[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckForwardDashInput()
        {
            if (!IsForwardInput(inputDown[0]))
                return false;
            
            for(int i = 1; i < fighterData.dashAllowFrame; i++)
            {
                if (IsBackwardInput(input[i]))
                {
                    return false;
                }

                if (IsForwardInput(input[i]))
                {
                    for(int j = i + 1; j < i + fighterData.dashAllowFrame; j++)
                    {
                        if (!IsForwardInput(input[j]) && !IsBackwardInput(input[j]))
                            return true;
                    }
                    return false;
                }
            }

            return false;
        }

        private bool CheckBackwardDashInput()
        {
            if (!IsBackwardInput(inputDown[0]))
                return false;

            for (int i = 1; i < fighterData.dashAllowFrame; i++)
            {
                if (IsForwardInput(input[i]))
                {
                    return false;
                }

                if (IsBackwardInput(input[i]))
                {
                    for (int j = i + 1; j < i + fighterData.dashAllowFrame; j++)
                    {
                        if (!IsForwardInput(input[j]) && !IsBackwardInput(input[j]))
                            return true;
                    }
                    return false;
                }
            }

            return false;
        }

        private bool IsAttackInput(int input)
        {
            return (input & (int)InputDefine.Attack) > 0;
        }

        private bool IsForwardInput(int input)
        {
            if(isFaceRight)
            {
                return (input & (int)InputDefine.Right) > 0;
            }
            else
            {
                return (input & (int)InputDefine.Left) > 0;
            }

        }

        private bool IsBackwardInput(int input)
        {
            if (isFaceRight)
            {
                return (input & (int)InputDefine.Left) > 0;
            }
            else
            {
                return (input & (int)InputDefine.Right) > 0;
            }

        }

        /// <summary>
        /// Copy data from current action and convert relative box position with fighter position
        /// </summary>
        private void ApplyCurrentActionData()
        {
            hitboxes.Clear();
            hurtboxes.Clear();

            foreach (var hitbox in fighterData.actions[currentActionID].GetHitboxData(currentActionFrame))
            {
                var box = new Hitbox();
                box.rect = TransformToFightRect(hitbox.rect, position, isFaceRight);
                box.proximity = hitbox.proximity;
                box.attackID = hitbox.attackID;
                hitboxes.Add(box);
            }

            foreach (var hurtbox in fighterData.actions[currentActionID].GetHurtboxData(currentActionFrame))
            {
                var box = new Hurtbox();
                Rect rect = hurtbox.useBaseRect ? fighterData.baseHurtBoxRect : hurtbox.rect;
                box.rect = TransformToFightRect(rect, position, isFaceRight);
                hurtboxes.Add(box);
            }

            var pushBoxData = fighterData.actions[currentActionID].GetPushboxData(currentActionFrame);
            pushbox = new Pushbox();
            Rect pushRect = pushBoxData.useBaseRect ? fighterData.basePushBoxRect : pushBoxData.rect;
            pushbox.rect = TransformToFightRect(pushRect, position, isFaceRight);
        }

        /// <summary>
        /// Convert relative box position with current fighter position
        /// </summary>
        /// <param name="dataRect"></param>
        /// <param name="basePosition"></param>
        /// <param name="isFaceRight"></param>
        /// <returns></returns>
        private Rect TransformToFightRect(Rect dataRect, Vector2 basePosition, bool isFaceRight)
        {
            var sign = isFaceRight ? 1 : -1;

            var fightPosRect = new Rect();
            fightPosRect.x = basePosition.x + (dataRect.x * sign);
            fightPosRect.y = basePosition.y + dataRect.y;
            fightPosRect.width = dataRect.width;
            fightPosRect.height = dataRect.height;

            return fightPosRect;
        }
    }
}