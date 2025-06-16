// using System;
// using System.Collections.Generic;
// using Mirror;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// public class FailedAlchemist : TestenemyHealth
// {
//     [Header("Alchemist Settings")]
//     [SerializeField] private float _transformationRange = 5f;
//     [SerializeField] private float _transformationCooldown = 10f;
//     [SerializeField] private float _transformationDuration = 15f;
//     [SerializeField] private List<GameObject> _transformationPrefabs; // Префабы животных/растений/слизней
//     
//     private float _lastTransformationTime;
//     
//     protected override void ServerUpdate()
//     {
//         base.ServerUpdate();
//         
//         // Проверяем возможность трансформации
//         if (Time.time > _lastTransformationTime + _transformationCooldown && !IsDead)
//         {
//             TryTransformPlayers();
//             _lastTransformationTime = Time.time;
//         }
//     }
//     
//     [Server]
//     private void TryTransformPlayers()
//     {
//         Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, _transformationRange);
//         
//         foreach (var hit in hitPlayers)
//         {
//             PlayerStats player = hit.GetComponent<PlayerStats>();
//             if (player != null && player.IsPlayer && !player.GetComponent<TransformationEffect>())
//             {
//                 TransformPlayer(player);
//             }
//         }
//     }
//     
//     [Server]
//     private void TransformPlayer(PlayerStats player)
//     {
//         if (_transformationPrefabs.Count == 0) return;
//         
//         // Выбираем случайный префаб для трансформации
//         int randomIndex = Random.Range(0, _transformationPrefabs.Count);
//         GameObject transformationPrefab = _transformationPrefabs[randomIndex];
//         
//         // Применяем эффект трансформации
//         player.GetComponent<NetworkIdentity>().connectionToClient.Send(new TransformationMessage
//         {
//             prefabName = transformationPrefab.name,
//             duration = _transformationDuration
//         });
//         
//         Debug.Log($"Transformed player {player.name} into {transformationPrefab.name}");
//     }
// }
//
// // Сообщение для трансформации
// public struct TransformationMessage : NetworkMessage
// {
//     public string prefabName;
//     public float duration;
// }
//
// // Компонент эффекта трансформации
// public class TransformationEffect : NetworkBehaviour
// {
//     [SyncVar] private float _duration;
//     [SyncVar] private string _originalPrefabName;
//     private GameObject _originalPlayerModel;
//     private GameObject _transformedModel;
//     
//     private void Start()
//     {
//         if (isServer)
//         {
//             // Сохраняем оригинальную модель игрока
//             _originalPlayerModel = transform.Find("Model").gameObject;
//             _originalPlayerModel.SetActive(false);
//             
//             // Создаем трансформированную модель
//             GameObject prefab = Resources.Load<GameObject>(_originalPrefabName);
//             _transformedModel = Instantiate(prefab, transform);
//             
//             // Настраиваем NetworkTransform для синхронизации
//             var netTransform = _transformedModel.AddComponent<NetworkTransform>();
//             netTransform.syncDirection = true;
//             netTransform.syncRotation = true;
//             netTransform.syncScale = true;
//             
//             NetworkServer.Spawn(_transformedModel, connectionToClient);
//         }
//         
//         // Запускаем таймер на клиенте
//         if (isClient)
//         {
//             StartCoroutine(TransformationTimer());
//         }
//     }
//     
//     [Client]
//     private IEnumerator TransformationTimer()
//     {
//         float timer = 0f;
//         
//         while (timer < _duration)
//         {
//             timer += Time.deltaTime;
//             yield return null;
//         }
//         
//         // После истечения времени возвращаем оригинальную форму
//         if (isServer)
//         {
//             ReturnToNormal();
//         }
//     }
//     
//     [Server]
//     public void ReturnToNormal()
//     {
//         if (_originalPlayerModel != null)
//         {
//             _originalPlayerModel.SetActive(true);
//         }
//         
//         if (_transformedModel != null)
//         {
//             NetworkServer.Destroy(_transformedModel);
//         }
//         
//         NetworkServer.Destroy(gameObject);
//     }
//     
//     [Server]
//     public static void ApplyTransformation(NetworkConnection conn, string prefabName, float duration)
//     {
//         GameObject effectObj = new GameObject("TransformationEffect");
//         TransformationEffect effect = effectObj.AddComponent<TransformationEffect>();
//         effect._duration = duration;
//         effect._originalPrefabName = prefabName;
//         
//         NetworkServer.Spawn(effectObj, conn);
//     }
// }
//
// // Обработчик сообщений трансформации
// public class TransformationMessageHandler : NetworkBehaviour
// {
//     public override void OnStartServer()
//     {
//         base.OnStartServer();
//         NetworkServer.RegisterHandler<TransformationMessage>(OnTransformationMessage);
//     }
//     
//     private void OnTransformationMessage(NetworkConnection conn, TransformationMessage message)
//     {
//         TransformationEffect.ApplyTransformation(conn, message.prefabName, message.duration);
//     }
// }