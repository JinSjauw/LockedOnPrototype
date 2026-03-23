using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

namespace BTreeEditor 
{
    public enum MethodID 
    {
        NONE = 0,
        DETECTED_ENEMY = 1,
        MOVE_TO_DESTINATION = 2,
        SELECT_NEXT_PATROL_POINT = 3,
        WAIT = 4,
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BTreeMethodAttribute : Attribute 
    {
        public MethodID methodID;
        public BTreeMethodAttribute(MethodID methodID) => this.methodID = methodID;
    }

    public delegate NodeState BehaviorMethod(BlackBoard blackBoard, ref NodeData nodeData);

    public static class MethodRegistry
    {
        private static Dictionary<MethodID, BehaviorMethod> methodRegistry = new Dictionary<MethodID, BehaviorMethod>();

        //Populate methodRegistry with all marked methods
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize() 
        {
            Debug.Log("Registering!: ");
            methodRegistry.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                // Get all types in the assembly
                foreach (var type in assembly.GetTypes())
                {
                    // Get all methods of the type
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    {
                        // Check for our custom attribute
                        var attr = method.GetCustomAttribute<BTreeMethodAttribute>();
                        if (attr != null)
                        {
                            // Create a delegate from the method
                            var del = (BehaviorMethod)Delegate.CreateDelegate(typeof(BehaviorMethod), method);

                            if (methodRegistry.ContainsKey(attr.methodID)) 
                            {
                                Debug.LogError($"{attr.methodID} HAS ALREADY BEEN REGISTERED!");
                                continue;
                            }

                            methodRegistry.Add(attr.methodID, del);
                            Debug.Log($"Registered: {attr.methodID}");
                        }
                    }
                }
            }
        }

        public static BehaviorMethod GetMethod(MethodID methodID) 
        {
            if(methodID == MethodID.NONE) return null;

            if(!methodRegistry.TryGetValue(methodID, out BehaviorMethod method)) 
            {
            #if UNITY_EDITOR
                throw new KeyNotFoundException($"Missing entry ID: {methodID}");
            #else
                Debug.LogError($"Missing entry ID: {methodID}");
                return null; // or a pre-allocated static empty array
            #endif
            }
            return method;
        } 
    }

    ////TEST CLASS -> REMOVE LATER
    public class TestMethods
    {
        [BTreeMethod(MethodID.DETECTED_ENEMY)]
        public static NodeState DetectEnemy(BlackBoard blackBoard, ref NodeData nodeData) 
        {
            EnemyController enemy = (EnemyController)blackBoard.GetField(BlackBoardFields.ENEMYCONTROLLER_COMPONENT);

            if (enemy.DetectedEnemy()) 
            {
                return NodeState.SUCCESS;
            }

            return NodeState.FAILURE;
        }

        [BTreeMethod(MethodID.SELECT_NEXT_PATROL_POINT)]
        public static NodeState SelectNextPatrolPoint(BlackBoard blackBoard, ref NodeData nodeData) 
        {
            EnemyController enemy = (EnemyController)blackBoard.GetField(BlackBoardFields.ENEMYCONTROLLER_COMPONENT);

            if (enemy.SetPatrolPoint(blackBoard)) 
            {
                return NodeState.SUCCESS;
            }

            return NodeState.FAILURE;
        }

        [BTreeMethod(MethodID.MOVE_TO_DESTINATION)]
        public static NodeState MoveToDestination(BlackBoard blackBoard, ref NodeData nodeData) 
        {
            EnemyController enemy = (EnemyController)blackBoard.GetField(BlackBoardFields.ENEMYCONTROLLER_COMPONENT);

            NodeState state = NodeState.FAILURE;

            enemy.MoveToDestination(ref state);

            return state;
        }

        [BTreeMethod(MethodID.WAIT)]
        public static NodeState WaitWorld(BlackBoard blackBoard, ref NodeData nodeData)
        {
            Debug.Log("Wait...(");

            if (blackBoard != null)
            {
                float time = (float)blackBoard.GetField(BlackBoardFields.WAIT_TIMER_FLOAT);

                if (time < 3)
                {
                    time += Time.deltaTime;

                    Debug.Log("Waiting! " + time);
                    blackBoard.UpdateField(BlackBoardFields.WAIT_TIMER_FLOAT, time);


                    return NodeState.RUNNING;
                }
                else
                {
                    time = 0;
                    Debug.Log("Done waiting! " + time);
                    blackBoard.UpdateField(BlackBoardFields.WAIT_TIMER_FLOAT, time);

                    return NodeState.SUCCESS;
                }
            }

            return NodeState.FAILURE;
        }
    }
}
