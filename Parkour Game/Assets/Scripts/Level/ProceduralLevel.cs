using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Level
{
    public class ProceduralLevel : MonoBehaviour
    {
        [SerializeField] private LevelGate _levelGate;
        [SerializeField] private LevelBlock _startBlock;
        [SerializeField] private LevelBlock[] _levelBlocks;
        [SerializeField] private int _aheadBlocks;
#if UNITY_EDITOR
        [AssetSelector("/Level Blocks", "prefab")] [SerializeField]
        private string _firstBlock;
#endif
        private LevelGate _levelGateA;
        private LevelGate _levelGateB;
        private bool _usingBuffer;

        private LevelBlock _lastEnqueued;
        private Queue<LevelBlock> _currentBlocks;
        private List<LevelBlock> _instantiatedLevelBlocks;
        private List<LevelBlock> _spawnedBlocks;
        
        private LevelGate LevelGate => _usingBuffer ? _levelGateA : _levelGateB;

        private void Awake()
        {
            //Spawn a copy of the gate as a buffer
            _levelGateA = _levelGate;
            _levelGateB = Instantiate(_levelGate, transform);
            
            //Initial setup of the block queue and lists
            _currentBlocks = new Queue<LevelBlock>(_aheadBlocks);
            _currentBlocks.Enqueue(_startBlock);
            _lastEnqueued = _startBlock;
            _instantiatedLevelBlocks = new List<LevelBlock>(_levelBlocks.Length);
            _spawnedBlocks = new List<LevelBlock>(_levelBlocks.Length);
            
            //Spawn in all the level blocks and hide them
            var index = 0;
            foreach (var levelBlock in _levelBlocks)
            {
                var newBlock = Instantiate(levelBlock, transform);
                newBlock.gameObject.SetActive(false);
                newBlock.BlockIndex = index++;
                _instantiatedLevelBlocks.Add(newBlock);
            }
            
            //Set the first gates position relative to the first block
            _levelGateA.ResetGate(_currentBlocks.Peek().EndGatePosition, SpawnNextBlock);
            
            //Set up the blocks ahead of initial block
            for (var i = 0; i < _aheadBlocks; i++)
            {
                var levelIndex = 0;
#if UNITY_EDITOR
                if (i == 0 && _firstBlock != "None")
                {
                    levelIndex = _instantiatedLevelBlocks.FindIndex(p => p.name == _firstBlock);
                }
                else
                {
                    levelIndex = Random.Range(0, _instantiatedLevelBlocks.Count);
                }
#else
                var levelIndex = Random.Range(0, _instantiatedLevelBlocks.Count);
#endif
                var block = _instantiatedLevelBlocks[levelIndex];
                _instantiatedLevelBlocks.RemoveAt(levelIndex);
                _currentBlocks.Enqueue(block);
                _spawnedBlocks.Add(block);
                block.Place(_lastEnqueued.EndGatePosition);
                _lastEnqueued = block;
            }
        }

        private void SpawnNextBlock()
        {
            //Reset the random bag
            if (_instantiatedLevelBlocks.Count <= _currentBlocks.Count)
            {
                _instantiatedLevelBlocks.AddRange(_spawnedBlocks);
                _spawnedBlocks.Clear();
            }
            
            //Flip the gate buffer so it alternates
            _usingBuffer = !_usingBuffer;
            var gate = LevelGate;
            
            //Choose random level block from the bag
            var randomIndex = Random.Range(0, _instantiatedLevelBlocks.Count);
            
            //Ensure we don't pick blocks that are spawned)
            var breakIndex = randomIndex;
            while (_currentBlocks.Contains(_instantiatedLevelBlocks[randomIndex]))
            {
                randomIndex = (randomIndex + 1) % _instantiatedLevelBlocks.Count;
                if (randomIndex == breakIndex)
                {
                    Debug.LogError("There's not enough level blocks to spawn another unique level block");
                    break;
                }
            }
            
            //Remove the level block from the bag
            var block = _instantiatedLevelBlocks[randomIndex];
            _instantiatedLevelBlocks.RemoveAt(randomIndex);
            _currentBlocks.Enqueue(block);
            _currentBlocks.Dequeue();
            _spawnedBlocks.Add(block);
            
            //Grab the last segment and place next the block at the end
            block.Place(_lastEnqueued.EndGatePosition);
            gate.ResetGate(_currentBlocks.Peek().EndGatePosition, SpawnNextBlock);
            _lastEnqueued = block;
        }

        private void OnValidate()
        {
            //Fetch all the level blocks in the Level Blocks folder
#if UNITY_EDITOR
            _aheadBlocks = Mathf.Min(_aheadBlocks, _levelBlocks.Length - 3);
            
            var levels = Directory.GetFiles(Application.dataPath + "/Level Blocks/", "*.prefab");
            for (var index = 0; index < levels.Length; index++)
            {
                levels[index] = Path.GetFileNameWithoutExtension(levels[index]);
            }
            _levelBlocks = new LevelBlock[levels.Length];
            for (var index = 0; index < levels.Length; index++)
            {
                var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Level Blocks/" + levels[index] + ".prefab");
                _levelBlocks[index] = go.GetComponent<LevelBlock>();
            }
#endif
        }
    }
}