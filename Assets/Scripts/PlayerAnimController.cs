using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController),typeof(AnimancerComponent))]
public class PlayerAnimController : MonoBehaviour
{
    public AnimancerComponent animancer;

    public MixerTransition2DAsset move;
    public MixerTransition2D run;

    PlayerMovementController movementController;
    DirectionalMixerState _move;
    // Start is called before the first frame update
    void Start()
    {
        movementController = this.GetComponent<PlayerMovementController>();
        _move = (DirectionalMixerState)animancer.States.GetOrCreate(move);
        animancer.Play(move);
    }

    // Update is called once per frame
    void Update()
    {
        _move.Parameter = movementController.GetMovementVector();
    }
}
