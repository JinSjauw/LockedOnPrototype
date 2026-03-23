using System.Collections.Generic;
using UnityEngine;

namespace BTreeEditor 
{
    //This tree runner script will be where the tree gets ran. Execution logic lives elsewhere. Tick/Update calls happen here. 
    public class TreeRunner : MonoBehaviour
    {

        [SerializeField] private BlackBoard blackBoard;

        //Need a SO for the treeAsset with both the root node and flattened tree node array!

        [SerializeField] private BehaviourTreeAsset treeAsset;
        [SerializeField] private NodeData[] nodeDatas;

        private TreeEvaluator evaluator;

        private bool initialized = false;

        public void Initialize()
        {
            if (treeAsset == null) return;

            treeAsset.Initialize();

            blackBoard.RegisterFields();

            TreeBaker.BakeTree(treeAsset.rootCopy, ref nodeDatas);
            evaluator = new TreeEvaluator(nodeDatas);

            initialized = true;
        }

        private void Update()
        {
            if (!initialized) return;
            //evaluator.Tick()
            evaluator.Evaluate(blackBoard);
        }

        private void OnDisable()
        {
            initialized = false;
        }

        private void OnDestroy()
        {
            if (treeAsset == null) return;

            treeAsset.ClearNodes();
        }
    }
}

