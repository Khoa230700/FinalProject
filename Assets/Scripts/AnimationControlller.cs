using UnityEngine;

public class AnimationControlller : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private MoveMent moveMent;
    //[SerializeField] private CameraLook cameraLook;

    void Update()
    {
        AnimatorCheckMove();
        AnimatorCheckCrouch();
        AnimatorCheckJump();
        AnimatorCheckReload();


    }
    public void AnimatorCheckMove()
    {
        animator.SetFloat("Vertical", moveMent.inputY);
        animator.SetFloat("Horizontal", moveMent.inputX);
        if (moveMent.Run() && moveMent.isRunning)
        {
            animator.SetBool("Run", true);
        }
        else if (!moveMent.isRunning)
        {
            animator.SetBool("Run", false); ;
        }
    }
    public void AnimatorCheckCrouch()
    {

        if (moveMent.Crouch())
        {
            animator.SetBool("Crouch", value: true);
            animator.SetFloat("SitHori", moveMent.inputX);
            animator.SetFloat("SitVerti", moveMent.inputY);
        }
        else if (!moveMent.Crouch())
        {
            animator.SetBool("Crouch", false);
        }
    }
    public void AnimatorCheckJump()
    {
        if (moveMent.Jump())
        {
            animator.Play("Jump");
        }
    }
    public void AnimatorCheckReload()
    {
        if (moveMent.Reload())
        {
            animator.Play("Reload");
        }
    }
}
