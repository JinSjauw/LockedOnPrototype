using BTreeEditor;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyAI 
{
    public class EnemyBlackBoard : BlackBoard
    {
        [SerializeField] private EnemyController enemyController;
        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

        private float moveSpeedFactor = 1.2f;
        private float waitTimer = 0;

        public override void RegisterFields()
        {
            blackBoardValues[BlackBoardFields.ENEMYCONTROLLER_COMPONENT] = enemyController;
            //Update and change from List<Transform> -> Vector3[] in future
            blackBoardValues[BlackBoardFields.PATROL_POINTS_TRANSFORMS_LIST] = patrolPoints;

            blackBoardValues[BlackBoardFields.MOVE_SPEED_FACTOR_FLOAT] = moveSpeedFactor;
            blackBoardValues[BlackBoardFields.WAIT_TIMER_FLOAT] = waitTimer;
        }

        public void SetPatrolPoints(List<Transform> patrolPoints)
        {
            this.patrolPoints = patrolPoints;
        }
    }
}


