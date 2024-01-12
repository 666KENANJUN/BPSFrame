
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
//*****************************************
//创建人：BPS
//创建时间：
//功能说明：
//*****************************************
namespace BPSFrame
{
    public static class ResManager
    {
        #region 普通class对象
        /// <summary>
        /// 获取实例-普通Class
        /// 如果类型需要缓存，会从对象池中获取
        /// 如果对象池没有或new一个返回
        /// <summary>
        public static T GetOrNew<T>() where T : class, new()
        {
            T obj = PoolSystem.GetObject<T>();
            obj ??= new T();
            return obj;
        }

        /// <summary>
        /// 获取实例-普通Class
        /// 如果类型需要缓存，会从对象池中获取
        /// 如果对象池没有或new一个返回
        /// <summary>
        /// <param name="keyName">对象池中的名称</param>
        public static T GetOrNew<T>(string keyName) where T : class, new()
        {
            T obj = PoolSystem.GetObject<T>(keyName);
            obj ??= new();
            return obj;
        }

        /// <summary>
        /// 卸载普通对象，这里是使用对象池的方式
        /// </summary>
        /// <param name="obj">object类型，基于类型存放</param>
        public static void PushObjectInPool(object obj)
        {
            PoolSystem.PushObject(obj);
        }

        /// <summary>
        /// 普通class对象（非GameObject）放置对象池中
        /// </summary>
        /// <param name="obj">object类型</param>
        /// <param name="keyName">基于KeyName存放</param>
        public static void PushObjectInPool(object obj, string keyName)
        {
            PoolSystem.PushObject(obj, keyName);
        }

        /// <summary>
        /// 初始化对象池并设置容量
        /// </summary>
        /// <typeparam name="T">new对象类型</typeparam>
        /// <param name="keyName">资源名称</param>
        /// <param name="maxCapacity">容量限制，超出时会销毁对象，而不是进入对象池，-1表示容量无限</param>
        /// <param name="defaultQuantity">默认容量，填写会向池子中放入对应数量的对象，0表示不预先放入</param>
        public static void InitObjectPool<T>(string keyName, int maxCapacity = -1, int defaultQuantity = 0) where T : new()
        {
            PoolSystem.InitObjectPool<T>(keyName, maxCapacity, defaultQuantity);
        }

        /// <summary>
        /// 初始化一个普通C#对象池类型
        /// </summary>
        /// <param name="keyName">keyName</param>
        /// <param name="maxCapacity">容量，超出时会丢弃而不是进入对象池，-1代表无限</param>
        public static void InitObjectPool(string keyName, int maxCapacity = -1)
        {
            PoolSystem.InitObjectPool(keyName, maxCapacity);
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <param name="maxCapacity">容量限制，超出时会销毁而不是进入对象池，-1代表无限</param>
        public static void InitObjectPool(Type type, int maxCapacity = -1)
        {
            PoolSystem.InitObjectPool(type, maxCapacity);
        }
        #endregion
        #region 游戏Asset
        /// <summary>
        /// 加载Unity资源，如AudioClip Sprite Prefab
        /// 要注意，资源不在使用的时候需要调用一次Release
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetName">加载的资源名</param>
        /// <returns>单个资源</returns>
        public static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            return Addressables.LoadAssetAsync<T>(assetName).WaitForCompletion();
        }

        /// <summary>
        /// 异步加载Unity资源 AudioClip Sprite GameObject(预制体)
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetName">资源名</param>
        /// <param name="callBack">回调函数</param>
        /// <returns></returns>
        public static async UniTask<T> LoadAssetAsync<T>(string assetName, Action<T> callBack = null) where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<T>(assetName);
            await handle.ToUniTask();
            callBack?.Invoke(handle.Result);
            return handle.Result;
        }

        /// <summary>
        /// 加载指定Key的所有资源
        /// 注意:批量加载时，如果释放资源要释放掉handle，直接去释放资源是无效的
        /// </summary>
        /// <typeparam name="T">加载类型</typeparam>
        /// <param name="keyName">一般是lable</param>
        /// <param name="handle">用来Release时使用</param>
        /// <param name="callBackOnEveryOne">注意这里是针对每一个资源的回调</param>
        /// <returns></returns>
        public static IList<T> LoadAssets<T>(string keyName, out AsyncOperationHandle<IList<T>> handle, Action<T> callBackOnEveryOne = null)
        {
            handle = Addressables.LoadAssetsAsync<T>(keyName, callBackOnEveryOne, true);
            return handle.WaitForCompletion();
        }

        /// <summary>
        /// 异步加载指定Key的所有资源
        /// 注意1:批量加载时，如果释放资源要释放掉handle，直接去释放资源是无效的
        /// 注意2:回调后使用callBack中的参数使用(.Result)即可访问资源列表
        /// </summary>
        /// <typeparam name="T">加载类型</typeparam>
        /// <param name="keyName">一般是lable</param>
        /// <param name="callBack">所有资源列表的统一回调，注意这是很必要的，因为Release时需要这个handle</param>
        /// <param name="callBackOnEvety">注意这里是针对每一个资源的回调,可以是Null</param>
        /// <returns></returns>
        public static async UniTask<IList<T>> LoadAssetsAsync<T>(string keyName, Action<AsyncOperationHandle<IList<T>>> callBack, Action<T> callBackOnEvety = null)
        {
            var handle = Addressables.LoadAssetsAsync<T>(keyName, callBackOnEvety);
            await handle.ToUniTask();
            callBack?.Invoke(handle);
            return handle.Result;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <typeparam name="T">对象的类型</typeparam>
        /// <param name="obj">具体对象</param>
        public static void UnloadAssset<T>(T obj)
        {
            Addressables.Release(obj);
        }

        /// <summary>
        /// 卸载因为批量加载而产生的handle
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="handle"></param>
        public static void UnLoadAssetsHandle<TObject>(AsyncOperationHandle<TObject> handle)
        {
            Addressables.Release(handle);
        }

        /// <summary>
        /// 销毁游戏物体并释放资源
        /// </summary>
        public static bool UnloadInstance(GameObject obj)
        {
            return Addressables.ReleaseInstance(obj);
        }
        #endregion
    }
}

