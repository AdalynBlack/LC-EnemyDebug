using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

namespace EnemyDebug.Input;

public class EnemyDebugInputs : LcInputActions 
{
    [InputAction(KeyboardControl.P, Name = "Freeze")]
    public InputAction FreezeEnemiesKey { get; set; }
}
