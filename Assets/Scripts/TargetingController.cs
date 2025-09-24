using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingController : MonoBehaviour
{
    #region Input Handling

    void OnScrollWheel(InputValue value) 
    {
        
    }

    void OnRightClick(InputValue value) 
    {
        if (value.isPressed) 
        {
            //Change Targeting

            //if it is held down have the target follow the mousePosition
        }
    }
    #endregion

    #region Unity

    #endregion

    #region Functions

    #endregion
}
