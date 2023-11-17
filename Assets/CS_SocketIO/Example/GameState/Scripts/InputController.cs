using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;


public class InputController : MonoBehaviour
{

    public static InputController _Instance { get; set; }
    public event Action<Axis> onAxisChange;

    public event Action OnSpellCast;

    private static Axis axis = new Axis { Horizontal = 0, Vertical =0};
    Axis LastAxis = new Axis { Horizontal = 0, Vertical =0};

    [SerializeField] private Animator animator;
    private SpriteRenderer playerSprite;
    void Start()
    {
        _Instance = this;
    }

    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        axis.Vertical = Mathf.RoundToInt(verticalInput);
        axis.Horizontal = Mathf.RoundToInt(horizontalInput);
        if(verticalInput != 0 || horizontalInput != 0 && animator != null)
        {
            animator.SetBool("Moving", true);
            if(horizontalInput > 0)
            {
                if (!playerSprite.flipX) playerSprite.flipX = true;
            }
            else if (horizontalInput < 0)
            {
                if (playerSprite.flipX) playerSprite.flipX = false;
            }
        }
        else if(animator != null)
        {
            animator.SetBool("Moving", false);
        }
        if (Input.GetButtonDown("Fire1"))
        {
            OnSpellCast?.Invoke();

        }
    }
    public void Setplayer(Animator animator, SpriteRenderer playerSprite)
    {
        this.animator = animator;
        this.playerSprite = playerSprite;
    }
    private void LateUpdate()
    {
        if (AxisChange())
        {
            LastAxis = new Axis { Horizontal = axis.Horizontal, Vertical = axis.Vertical };
            //NetworkController._Instance.Socket.Emit("move", axis);
            onAxisChange?.Invoke(axis);
        }
    }
 

    private bool AxisChange()
    {
        return (axis.Vertical != LastAxis.Vertical || axis.Horizontal !=LastAxis.Horizontal);
    }
}

public class Axis
{
    public int Horizontal;
    public int Vertical;
}


