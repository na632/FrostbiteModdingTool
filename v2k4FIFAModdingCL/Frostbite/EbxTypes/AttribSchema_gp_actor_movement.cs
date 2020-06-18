using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v2k4FIFAModding.Frostbite.EbxTypes
{
    public enum MovementSpeed
    {
        MOVE_VERYSLOW,
        MOVE_SLOW,
        MOVE_VANILLA,
        MOVE_FAST,
        MOVE_VERYFAST
    }

    public class AttribSchema_gp_actor_movement
    {
        static AttribSchema_gp_actor_movement _single;
        public static AttribSchema_gp_actor_movement GetAttrib()
        {
            if (_single == null)
                _single = new AttribSchema_gp_actor_movement();

            return _single;
        }

        public List<float> ATTR_DribbleJogSpeedModifier = new List<float>() { 0, 0, 0, 0, 0, 0, 0, 0.01f, 0.02f, 0.03f };
        public List<float> ATTR_SprintSpeedTbl = new List<float>();

        public float ATTR_DribbleJogSpeed = 0.2f;
        public float ATTR_JogSpeed = 0.3f;


        public MovementSpeed? movementSpeed;// = MovementSpeed.MOVE_VANILLA;
        public MovementSpeed MovementSpeed
        {
            get
            {
                if (movementSpeed == null)
                    movementSpeed = MovementSpeed.MOVE_VANILLA;

                return movementSpeed.Value;
            }
            set
            {
                movementSpeed = value;
                switch (movementSpeed)
                {
                    case MovementSpeed.MOVE_VERYSLOW:
                        ATTR_DribbleJogSpeed = 0.2f * 0.5f;
                        ATTR_JogSpeed = 0.3f * 0.5f;
                        ATTR_SprintSpeedTbl = new List<float>() { 0.1f, 0.5f };
                        break;
                    case MovementSpeed.MOVE_SLOW:
                        ATTR_DribbleJogSpeed = ATTR_DribbleJogSpeed * 0.75f;
                        ATTR_JogSpeed = ATTR_JogSpeed * 0.75f;
                        ATTR_SprintSpeedTbl = new List<float>() { 0.1f, 0.75f };
                        break;
                    case MovementSpeed.MOVE_VANILLA:
                        ATTR_DribbleJogSpeed = 0.2f;
                        ATTR_JogSpeed = 0.3f;
                        ATTR_SprintSpeedTbl = new List<float>() { 0.25f, 1.0f };
                        break;
                    case MovementSpeed.MOVE_FAST:
                        ATTR_DribbleJogSpeed = 0.2f * 1.25f;
                        ATTR_JogSpeed = 0.3f * 1.25f;
                        ATTR_SprintSpeedTbl = new List<float>() { 0.33f, 1.0f };
                        break;
                    case MovementSpeed.MOVE_VERYFAST:
                        ATTR_DribbleJogSpeed = 0.2f * 1.5f;
                        ATTR_JogSpeed = 0.3f * 1.5f;
                        ATTR_SprintSpeedTbl = new List<float>() { 0.5f, 1.0f };
                        break;
                }
            }
        }


    }
}
