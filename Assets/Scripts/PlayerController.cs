using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;



public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float moveSpeedIn = 5f;//�v���C���[�̈ړ����x�����
    [SerializeField]
    float dashMultiplier = 2f;

    Animator animator;
    Rigidbody playerRb;//�v���C���[��Rigidbody
    Status playerStatus = Status.GROUND;//�v���C���[�̏��

    float firstSpeed = 16.0f;//����
    const float gravity = 120.0f;//�d��
    const float jumpLowerLimit = 0.03f;//�W�����v���Ԃ̉���

    float timer = 0f;//�o�ߎ���
    bool jumpKey = false;//�W�����v�L�[
    bool keyLook = false;//�L�[���͂��󂯕t���Ȃ�


    Vector3 moveSpeed;//�v���C���[�̈ړ����x

    Vector3 currentPos;//�v���C���[�̌��݂̈ʒu
    Vector3 pastPos;//�v���C���[�̉ߋ��̈ʒu

    Vector3 delta;//�v���C���[�̈ړ���

    Quaternion playerRot;//�v���C���[�̐i�s�����������N�H�[�^�j�I��

    float currentAngularVelocity;//���݂̉�]�e���x

    [SerializeField]
    float maxAngularVelocity = Mathf.Infinity;//�ő�̉�]�p���x[deg/s]

    [SerializeField]
    float smoothTime = 0.1f;//�i�s�����ɂ����邨���悻�̎���[s]

    float diffAngle;//���݂̌����Ɛi�s�����̊p�x

    float rotAngle;//���݂̉�]����p�x

    Quaternion nextRot;//�ǂ񂭂炢��]���邩

    void Start()
    {
        animator = GetComponent<Animator>();

        playerRb = GetComponent<Rigidbody>();

        pastPos = transform.position;
    }

    void Update()
    {
        //------�v���C���[�̈ړ�------

        //�J�����ɑ΂��đO���擾
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        //�J�����ɑ΂��ĉE���擾
        Vector3 cameraRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;


        float currentSpeed = moveSpeedIn;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= dashMultiplier;
        }

        //moveVelocity��0�ŏ���������
        moveSpeed = Vector3.zero;

        //�ړ�����
        if (Input.GetKey(KeyCode.W))
        {
            moveSpeed += currentSpeed * cameraForward;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveSpeed  += -currentSpeed * cameraRight;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveSpeed  += -currentSpeed * cameraForward;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveSpeed  += currentSpeed * cameraRight;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            jumpKey = !keyLook;
        }
        else
        {
            jumpKey = false;
            keyLook = false;
        }

        //Move���\�b�h�ŁA�͉����Ă��炤
        Move();

        //����������
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D))
        {
            //playerRb.velocity = Vector3.zero;
            // playerRb.angularVelocity = Vector3.zero;
        }

        //------�v���C���[�̉�]------

        //���݂̈ʒu
        currentPos = transform.position;

        //�ړ��ʌv�Z
        delta = currentPos - pastPos;
        delta.y = 0;

        //�ߋ��̈ʒu�̍X�V
        pastPos = currentPos;

        if (delta == Vector3.zero)
            return;

        playerRot = Quaternion.LookRotation(delta, Vector3.up);

        diffAngle = Vector3.Angle(transform.forward, delta);

        //Vector3.SmoothDamp��Vector3�^�̒l�����X�ɕω�������
        //Vector3.SmoothDamp (���ݒn, �ړI�n, ref ���݂̑��x, �J�ڎ���, �ō����x);
        rotAngle = Mathf.SmoothDampAngle(0, diffAngle, ref currentAngularVelocity, smoothTime, maxAngularVelocity);

        nextRot = Quaternion.RotateTowards(transform.rotation, playerRot, rotAngle);

        transform.rotation = nextRot;

        //�A�j���[�^�[�ݒ�
        if (moveSpeed.magnitude > 0.1f) //���͂�����Ƃ�������������
        {
            playerRb.MovePosition(transform.position + moveSpeed.normalized * currentSpeed * Time.deltaTime);

            if (currentSpeed > moveSpeedIn)
            {
                animator.SetBool("Run", true);
                animator.SetBool("walk", false);
            }
            else if (currentSpeed > 0.1f)
            {
                animator.SetBool("walk", true);
                animator.SetBool("Run", false);
            }
        }
        else
        {
            animator.SetBool("walk", false);
            animator.SetBool("Run", false);
        }
    }

    /// <summary>
    /// �ړ������ɗ͂�������
    /// </summary>
    private void Move()
    {
        //playerRb.AddForce(moveSpeed, ForceMode.Force);

        playerRb.velocity = moveSpeed;
    }
    void FixedUpdate()
    {
        Vector3 newvec = Vector3.zero;

        switch(playerStatus)
        {
            case Status.GROUND:
                if(jumpKey)
                {
                    playerStatus = Status.UP;
                }
                break;

            case Status.UP:
                timer += Time.deltaTime;

                if(jumpKey || jumpLowerLimit > timer)
                {
                    newvec.y = firstSpeed;
                    newvec.y -= (gravity * Mathf.Pow(timer, 2));
                }

                else
                {
                    timer += Time.deltaTime;
                    newvec.y = firstSpeed;
                    newvec.y -= (gravity * Mathf.Pow(timer, 2));
                }

                if (0f > newvec.y)
                {
                    playerStatus = Status.DOWN;
                    newvec.y = 0f;
                    timer = 0.1f;
                }
                break;

            case Status.DOWN:
                timer += Time.deltaTime;

                newvec.y = 0f;
                newvec.y = -(gravity * Mathf.Pow(timer, 2));
                break;

            default:
                break;
        }

        Vector3 finalVelocity = moveSpeed;
        finalVelocity.y = newvec.y;

        playerRb.velocity = finalVelocity;
    }

    void OnCollisionStay(Collision collision)
    {
        if(playerStatus == Status.DOWN &&
            collision.gameObject.name.Contains("Ground"))
        {
            playerStatus = Status.GROUND;
            timer = 0f;
            keyLook = true;
        }
    }
}
