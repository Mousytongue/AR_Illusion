using GameSparks.RT;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ASL
{
    /// <summary>
    /// Provides functions pertaining to ASL to be called by the user but not linked to any specific object
    /// https://codelabs.developers.google.com/codelabs/arcore-cloud-anchors/index.html#4
    /// https://developers.google.com/ar/develop/developer-guides/anchors
    /// https://developers.google.com/ar/develop/unity/cloud-anchors/quickstart-unity-android
    /// https://developers.google.com/ar/develop/unity/cloud-anchors/overview-unity
    /// </summary>
    public static class ASLHelper
    {
        /// <summary>
        /// A dictionary containing all of the ASLObjects in a scene
        /// </summary>
        static public Dictionary<string, ASLObject> m_ASLObjects = new Dictionary<string, ASLObject>();

        /// <summary>A struct that contains information about the CloudAnchor to allow it to be stored in Cloud Anchors dictionary (m_CloudAnchors)</summary>
        public struct CloudAnchor
        {
            XPAnchor anchor;
            bool worldOrigin;

            public CloudAnchor(XPAnchor _anchor, bool _worldOrigin)
            {
                anchor = _anchor;
                worldOrigin = _worldOrigin;
            }
        }

        /// <summary>
        /// A dictionary containing all of the Cloud Anchors in a scene
        /// </summary>
        static public Dictionary<string, CloudAnchor> m_CloudAnchors = new Dictionary<string, CloudAnchor>();

        #region Instantiation

        #region Primitive Instantiation

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_type">The primitive type to be instantiated</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity);
        /// }
        /// </code></example>
        static public void InstanitateASLObject(PrimitiveType _type, Vector3 _position, Quaternion _rotation)
        {
            SendSpawnPrimitive(_type, _position, _rotation);
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_type">The primitive type to be instantiated</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Where gameObject is the parent of the object that is being created here
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id); 
        /// }
        /// </code>
        /// <code>
        /// void SomeOtherFunction()
        /// {
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, ""); //Using this overload (and others) and passing in an empty string is a valid option 
        /// }
        /// </code>
        /// <code>
        /// void WoahAnotherFunction()
        /// {
        ///     //Where 'MyParentObject' is the name of the parent of the object that is being created here
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, "MyParentObject"); 
        ///     //The parent of your object can also be found by passing in the name (e.g., gameObject.name) of the parent you want. However, this method is a lot slower than using the ASL ID method, 
        ///     //but it is the only way to assign a non-ASL Object as a parent to an ASL Object for all users
        /// }
        /// </code></example>
        static public void InstanitateASLObject(PrimitiveType _type, Vector3 _position, Quaternion _rotation, string _parentID)
        {
            SendSpawnPrimitive(_type, _position, _rotation, _parentID ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_type">The primitive type to be instantiated</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.
        /// When you use a Unity component, things are slightly different. For example, to add a Rigidbody component, you would enter this: "UnityEngine.Rigidbody,UnityEngine"
        /// In this case, the pattern is still the namespace + component, but then it is followed with by ",UnityEngine". If you need UnityEditor, try that after the comma</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Using just the name space and the class name should be enough for _componentAssemblyQualifiedName. For more info, go here
        ///     //https://docs.microsoft.com/en-us/dotnet/api/system.type.assemblyqualifiedname?view=netframework-4.8#System_Type_AssemblyQualifiedName
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id, "MyNamespace.MyComponent"); 
        ///     
        ///     //Note: If you need to add more than 1 component to an object, you will need to use the _aslGameObjectCreatedCallbackInfo parameter as well. 
        ///     //See those instantiation overload options for more details
        /// }
        /// </code>
        ///</example>
        static public void InstanitateASLObject(PrimitiveType _type, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName)
        {
            SendSpawnPrimitive(_type, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_type">The primitive type to be instantiated</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_aslGameObjectCreatedCallbackInfo">This is the function that you want to be called after object creation</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Where gameObject is the parent of the object that is being created here and "MyNamespace.MyClass" is an example of a component you want to add
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id,
        ///     "MyNamespace.MyClass",
        ///     MyUponInstantiationFunction);
        /// }
        /// public static void MyUponInstantiationFunction(GameObject _myGameObject)
        /// {
        ///     Debug.Log("Caller-Object ID: " + _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().m_Id);
        /// }
        /// 
        /// </code></example>
        static public void InstanitateASLObject(PrimitiveType _type, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName,
            ASLObject.ASLGameObjectCreatedCallback _aslGameObjectCreatedCallbackInfo)
        {
            SendSpawnPrimitive(_type, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "", 
                _aslGameObjectCreatedCallbackInfo?.Method?.ReflectedType?.ToString() ?? "", _aslGameObjectCreatedCallbackInfo?.Method?.Name ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_type">The primitive type to be instantiated</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_aslGameObjectCreatedCallbackInfo">This is the function that you want to be called after object creation</param>
        /// <param name="_aslClaimCancelledRecoveryFunctionInfo">This is the function that you want to be called whenever a claim is rejected/cancelled</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Where gameObject is the parent of the object that is being created here and "MyNamespace.MyClass" is an example of a component you want to add
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id,
        ///     "MyNamespace.MyClass",
        ///     MyUponInstantiationFunction, 
        ///     MyClaimRejectedFunction); 
        ///  }
        /// public static void MyUponInstantiationFunction(GameObject _myGameObject)
        /// {
        ///     Debug.Log("Caller-Object ID: " + _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().m_Id);
        /// }
        /// 
        /// public static void MyClaimRejectedFunction(string _id, int _cancelledCallbacks)
        /// {
        ///    Debug.LogWarning("We are going to cancel " + _cancelledCallbacks +
        ///       " callbacks generated by a claim for object: " + _id + " rather than try to recover.");
        /// }
        /// </code></example>
        static public void InstanitateASLObject(PrimitiveType _type, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName,
            ASLObject.ASLGameObjectCreatedCallback _aslGameObjectCreatedCallbackInfo,
            ASLObject.ClaimCancelledRecoveryCallback _aslClaimCancelledRecoveryFunctionInfo)
        {
            SendSpawnPrimitive(_type, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "",
                _aslGameObjectCreatedCallbackInfo?.Method?.ReflectedType?.ToString() ?? "", _aslGameObjectCreatedCallbackInfo?.Method?.Name ?? "",
                _aslClaimCancelledRecoveryFunctionInfo?.Method?.ReflectedType?.ToString() ?? "", _aslClaimCancelledRecoveryFunctionInfo?.Method?.Name ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_type">The primitive type to be instantiated</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_aslGameObjectCreatedCallbackInfo">This is the function that you want to be called after object creation</param>
        /// <param name="_aslClaimCancelledRecoveryFunctionInfo">This is the function that you want to be called whenever a claim is rejected/cancelled</param>
        /// <param name="_aslFloatFunctionInfo">This is the name of the function that you want to be executed whenever you use the 
        /// <see cref="ASLObject.SendFloat4(float[])"/> function.</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Where gameObject is the parent of the object that is being created here and "MyNamespace.MyClass" is an example of a component you want to add
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id,
        ///     "MyNamespace.MyClass",
        ///     MyUponInstantiationFunction, 
        ///     MyClaimRejectedFunction,
        ///     MySendFloatFunction); 
        /// }
        /// public static void MyUponInstantiationFunction(GameObject _myGameObject)
        /// {
        ///     Debug.Log("Caller-Object ID: " + _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().m_Id);
        ///     _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         float[] myFloats = new float[] { 1.1f, 2.5f, 3.4f, 4.9f };
        ///         _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().SendFloat4(myFloats);
        ///     });
        /// }
        /// 
        /// public static void MyClaimRejectedFunction(string _id, int _cancelledCallbacks)
        /// {
        ///    Debug.LogWarning("We are going to cancel " + _cancelledCallbacks +
        ///       " callbacks generated by a claim for object: " + _id + " rather than try to recover.");
        /// } 
        /// public static void MySendFloatFunction(float[] _floats)
        /// {
        ///     for (int i = 0; i&lt;_floats.Length; i++)
        ///     {
        ///         Debug.Log("Value Sent: " + _floats[i]);
        ///     }
        /// }
        /// </code></example>
        static public void InstanitateASLObject(PrimitiveType _type, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName,
            ASLObject.ASLGameObjectCreatedCallback _aslGameObjectCreatedCallbackInfo,
            ASLObject.ClaimCancelledRecoveryCallback _aslClaimCancelledRecoveryFunctionInfo,
            ASLObject.FloatCallback _aslFloatFunctionInfo)
        {
            SendSpawnPrimitive(_type, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "", 
                _aslGameObjectCreatedCallbackInfo?.Method?.ReflectedType.ToString() ?? "", _aslGameObjectCreatedCallbackInfo?.Method?.Name ?? "",
                _aslClaimCancelledRecoveryFunctionInfo?.Method?.ReflectedType?.ToString() ?? "", _aslClaimCancelledRecoveryFunctionInfo?.Method?.Name ?? "",
                _aslFloatFunctionInfo?.Method?.ReflectedType?.ToString() ?? "", _aslFloatFunctionInfo?.Method?.Name ?? "");
        }

        #endregion

        #region Prefab Instantiation

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_prefabName">The name of the prefab to be instantiated. Make sure your prefab is located in the Resources/Prefabs folder so it can be found</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     ASL.ASLHelper.InstanitateASLObject("MyPrefab", new Vector3(0, 0, 0), Quaternion.identity);
        /// }
        /// </code></example>
        static public void InstanitateASLObject(string _prefabName, Vector3 _position, Quaternion _rotation)
        {
            SendSpawnPrefab(_prefabName, _position, _rotation);
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_prefabName">The name of the prefab to be instantiated. Make sure your prefab is located in the Resources/Prefabs folder so it can be found</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <example><code>
        /// //Where gameObject is the parent of the object that is being created here
        /// ASL.ASLHelper.InstanitateASLObject("MyPrefab", new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id); 
        /// </code>
        /// <code>
        /// void SomeFunction()
        /// {
        ///     ASL.ASLHelper.InstanitateASLObject("MyPrefab", new Vector3(0, 0, 0), Quaternion.identity, ""); //Using this overload (and others) and passing in an empty string is a valid option 
        /// }
        /// </code>
        /// <code>
        /// void SomeOtherFunction()
        /// {
        ///     //Where 'MyParentObject' is the name of the parent of the object that is being created here
        ///     ASL.ASLHelper.InstanitateASLObject("MyPrefab", new Vector3(0, 0, 0), Quaternion.identity, "MyParentObject"); 
        ///     //The parent of your object can also be found by passing in the name (e.g., gameObject.name) of the parent you want. However, this method is a lot slower than using the ASL ID method, 
        ///     //but it is the only way to assign a non-ASL Object as a parent to an ASL Object for all users
        /// }
        /// </code></example>
        static public void InstanitateASLObject(string _prefabName, Vector3 _position, Quaternion _rotation, string _parentID)
        {
            SendSpawnPrefab(_prefabName, _position, _rotation, _parentID ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_prefabName">The name of the prefab to be instantiated. Make sure your prefab is located in the Resources/Prefabs folder so it can be found</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.
        /// When you use a Unity component, things are slightly different. For example, to add a Rigidbody component, you would enter this: "UnityEngine.Rigidbody,UnityEngine"
        /// In this case, the pattern is still the namespace + component, but then it is followed with by ",UnityEngine". If you need UnityEditor, try that after the comma</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Using just the name space and the class name should be enough for _componentAssemblyQualifiedName. For more info, go here
        ///     //https://docs.microsoft.com/en-us/dotnet/api/system.type.assemblyqualifiedname?view=netframework-4.8#System_Type_AssemblyQualifiedName
        ///     ASL.ASLHelper.InstanitateASLObject(PrimitiveType.Cube, new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id, "MyNamespace.MyComponent"); 
        ///     
        ///     //Note: If you need to add more than 1 component to an object, use the AddComponent ASL function after this object is created via an after creation function
        ///     //To do so, you will need to use the instantiatedGameObjectClassName and InstantiatedGameObjectFunctionName parameters as well. 
        ///     //See those instantiation overload options for more details
        /// }
        /// </code>
        ///</example>
        static public void InstanitateASLObject(string _prefabName, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName)
        {
            SendSpawnPrefab(_prefabName, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_prefabName">The name of the prefab to be instantiated. Make sure your prefab is located in the Resources/Prefabs folder so it can be found</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_aslGameObjectCreatedCallbackInfo">This is the function that you want to be called after object creation</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Where gameObject is the parent of the object that is being created here and "MyNamespace.MyClass" is an example of a component you want to add
        ///     ASL.ASLHelper.InstanitateASLObject("MyPrefab", new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id,
        ///     "MyNamespace.MyClass",
        ///     MyUponInstantiationFunction); 
        /// }
        /// public static void MyUponInstantiationFunction(GameObject _myGameObject)
        /// {
        ///     Debug.Log("Caller-Object ID: " + _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().m_Id);
        /// }
        /// 
        /// </code></example>
        static public void InstanitateASLObject(string _prefabName, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName,
            ASLObject.ASLGameObjectCreatedCallback _aslGameObjectCreatedCallbackInfo)
        {
            SendSpawnPrefab(_prefabName, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "", 
                _aslGameObjectCreatedCallbackInfo?.Method?.ReflectedType?.ToString() ?? "", _aslGameObjectCreatedCallbackInfo?.Method?.Name ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_prefabName">The name of the prefab to be instantiated. Make sure your prefab is located in the Resources/Prefabs folder so it can be found</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_aslGameObjectCreatedCallbackInfo">This is the function that you want to be called after object creation</param>
        /// <param name="_aslClaimCancelledRecoveryFunctionInfo">This is the function that you want to be called whenever a claim is rejected/cancelled</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Where gameObject is the parent of the object that is being created here and "MyNamespace.MyClass" is an example of a component you want to add
        ///     ASL.ASLHelper.InstanitateASLObject("MyPrefab", new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id,
        ///     "MyNamespace.MyClass",
        ///     MyUponInstantiationFunction,
        ///     MyClaimRejectedFunction);
        ///  }
        /// public static void MyUponInstantiationFunction(GameObject _myGameObject)
        /// {
        ///     Debug.Log("Caller-Object ID: " + _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().m_Id);
        /// }
        /// 
        /// public static void MyClaimRejectedFunction(string _id, int _cancelledCallbacks)
        /// {
        ///    Debug.LogWarning("We are going to cancel " + _cancelledCallbacks +
        ///       " callbacks generated by a claim for object: " + _id + " rather than try to recover.");
        /// } 
        /// 
        /// </code></example>
        static public void InstanitateASLObject(string _prefabName, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName,
            ASLObject.ASLGameObjectCreatedCallback _aslGameObjectCreatedCallbackInfo,
            ASLObject.ClaimCancelledRecoveryCallback _aslClaimCancelledRecoveryFunctionInfo)
        {
            SendSpawnPrefab(_prefabName, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "", 
                _aslGameObjectCreatedCallbackInfo?.Method?.ReflectedType?.ToString() ?? "", _aslGameObjectCreatedCallbackInfo?.Method?.Name ?? "",
                _aslClaimCancelledRecoveryFunctionInfo?.Method?.ReflectedType?.ToString() ?? "", _aslClaimCancelledRecoveryFunctionInfo?.Method?.Name ?? "");
        }

        /// <summary>
        /// Create an ASL Object
        /// </summary>
        /// <param name="_prefabName">The name of the prefab to be instantiated. Make sure your prefab is located in the Resources/Prefabs folder so it can be found</param>
        /// <param name="_position">The position where the object will be instantiated</param>
        /// <param name="_rotation">The rotation orientation of the object to be instantiated</param>
        /// <param name="_parentID">The id or name of the parent object for this instantiated object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_aslGameObjectCreatedCallbackInfo">This is the function that you want to be called after object creation</param>
        /// <param name="_aslClaimCancelledRecoveryFunctionInfo">This is the function that you want to be called whenever a claim is rejected/cancelled</param>
        /// <param name="_aslFloatFunctionInfo">This is the name of the function that you want to be executed whenever you use the 
        /// <see cref="ASLObject.SendFloat4(float[])"/> function.</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     //Where gameObject is the parent of the object that is being created here and "MyNamespace.MyClass" is an example of a component you want to add
        ///     ASL.ASLHelper.InstanitateASLObject("MyPrefab", new Vector3(0, 0, 0), Quaternion.identity, gameobject.GetComponent&lt;ASL.ASLObject&gt;().m_Id,
        ///     "MyNamespace.MyClass",
        ///     MyUponInstantiationFunction
        ///     MyClaimRejectedFunction,
        ///     MySendFloatFunction);
        /// }
        /// public static void MyUponInstantiationFunction(GameObject _myGameObject)
        /// {
        ///     Debug.Log("Caller-Object ID: " + _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().m_Id);
        ///     _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().SendAndSetClaim(() =>
        ///     {
        ///         float[] myFloats = new float[] { 1.1f, 2.5f, 3.4f, 4.9f };
        ///         _myGameObject.GetComponent&lt;ASL.ASLObject&gt;().SendFloat4(myFloats);
        ///     });
        /// }
        /// 
        /// public static void MyClaimRejectedFunction(string _id, int _cancelledCallbacks)
        /// {
        ///    Debug.LogWarning("We are going to cancel " + _cancelledCallbacks +
        ///       " callbacks generated by a claim for object: " + _id + " rather than try to recover.");
        /// } 
        /// public static void MySendFloatFunction(float[] _floats)
        /// {
        ///     for (int i = 0; i&lt;_floats.Length; i++)
        ///     {
        ///         Debug.Log("Value Sent: " + _floats[i]);
        ///     }
        /// } 
        /// 
        /// </code></example>
        static public void InstanitateASLObject(string _prefabName, Vector3 _position, Quaternion _rotation, string _parentID, string _componentAssemblyQualifiedName,
            ASLObject.ASLGameObjectCreatedCallback _aslGameObjectCreatedCallbackInfo,
            ASLObject.ClaimCancelledRecoveryCallback _aslClaimCancelledRecoveryFunctionInfo,
            ASLObject.FloatCallback _aslFloatFunctionInfo)
        {
            SendSpawnPrefab(_prefabName, _position, _rotation, _parentID ?? "", _componentAssemblyQualifiedName ?? "", 
                _aslGameObjectCreatedCallbackInfo?.Method?.ReflectedType?.ToString() ?? "", _aslGameObjectCreatedCallbackInfo?.Method?.Name ?? "",
                _aslClaimCancelledRecoveryFunctionInfo?.Method?.ReflectedType?.ToString() ?? "", _aslClaimCancelledRecoveryFunctionInfo?.Method?.Name ?? "",
                _aslFloatFunctionInfo?.Method?.ReflectedType?.ToString() ?? "", _aslFloatFunctionInfo?.Method?.Name ?? "");
        }


        #endregion

        #endregion

        /// <summary>
        /// Sends a packet out to all players to spawn an object based upon a prefab
        /// </summary>
        /// <param name="_type">The type of primitive to be spawned</param>
        /// <param name="_position">The position of where the object will be spawned</param>
        /// <param name="_rotation">The rotation orientation of the object upon spawn</param>
        /// <param name="_parentID">The id of the parent object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_instantiatedGameObjectClassName">The name of the class that contains the user provided function detailing what to do with this object after creation</param>
        /// <param name="_instantiatedGameObjectFunctionName">The name of the user provided function that contains the details of what to do with this object after creation</param>
        /// <param name="_claimRecoveryClassName">The name of the class that contains the user provided function detailing what to do if a claim for this object is rejected</param>
        /// <param name="_claimRecoveryFunctionName">The name of the user provided function that contains the details of what to do with this object if a claim for it is rejected</param>
        /// <param name="_sendFloatClassName">The name of the class that contains the user provided function detailing what to do when a user calls <see cref="ASLObject.SendFloat4(float[])"/></param>
        /// <param name="_sendFloatFunctionName">The name of the user provided function that contains the details of what to do with this object if a user calls <see cref="ASLObject.SendFloat4(float[])"/></param>
        private static void SendSpawnPrimitive
            (PrimitiveType _type, Vector3 _position, Quaternion _rotation, string _parentID = "", string _componentAssemblyQualifiedName = "", string _instantiatedGameObjectClassName = "", string _instantiatedGameObjectFunctionName = "", 
            string _claimRecoveryClassName = "", string _claimRecoveryFunctionName = "", string _sendFloatClassName = "", string _sendFloatFunctionName = "")
        {
            string guid = Guid.NewGuid().ToString();
            using (RTData data = RTData.Get())
            {
                data.SetString((int)GameController.DataCode.Id, guid);
                data.SetInt((int)GameController.DataCode.PrimitiveType, (int)_type);
                data.SetVector3((int)GameController.DataCode.Position, _position);
                data.SetVector4((int)GameController.DataCode.Rotation, new Vector4(_rotation.x, _rotation.y, _rotation.z, _rotation.w));
                data.SetString((int)GameController.DataCode.ParentId, _parentID);
                data.SetString((int)GameController.DataCode.ComponentName, _componentAssemblyQualifiedName);
                data.SetString((int)GameController.DataCode.InstantiatedGameObjectClassName, _instantiatedGameObjectClassName);
                data.SetString((int)GameController.DataCode.InstantiatedGameObjectFunctionName, _instantiatedGameObjectFunctionName);
                data.SetString((int)GameController.DataCode.ClaimRecoveryClassName, _claimRecoveryClassName);
                data.SetString((int)GameController.DataCode.ClaimRecoveryFunctionName, _claimRecoveryFunctionName);
                data.SetString((int)GameController.DataCode.SendFloatClassName, _sendFloatClassName);
                data.SetString((int)GameController.DataCode.SendFloatFunctionName, _sendFloatFunctionName);
                GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.SpawnPrimitive, GameSparksRT.DeliveryIntent.RELIABLE, data);
            }
        }

        /// <summary>
        /// Sends a packet out to all players to spawn a prefab object
        /// </summary>
        /// <param name="_prefabName">The name of the prefab to be used</param>
        /// <param name="_position">The position of where the object will be spawned</param>
        /// <param name="_rotation">The rotation orientation of the object upon spawn</param>
        /// <param name="_parentID">The id of the parent object</param>
        /// <param name="_componentAssemblyQualifiedName">The full name of the component to be added to this object upon creation.</param>
        /// <param name="_instantiatedGameObjectClassName">The name of the class that contains the user provided function detailing what to do with this object after creation</param>
        /// <param name="_instantiatedGameObjectFunctionName">The name of the user provided function that contains the details of what to do with this object after creation</param>
        /// <param name="_claimRecoveryClassName">The name of the class that contains the user provided function detailing what to do if a claim for this object is rejected</param>
        /// <param name="_claimRecoveryFunctionName">The name of the user provided function that contains the details of what to do with this object if a claim for it is rejected</param>
        /// <param name="_sendFloatClassName">The name of the class that contains the user provided function detailing what to do when a user calls <see cref="ASLObject.SendFloat4(float[])"/></param>
        /// <param name="_sendFloatFunctionName">The name of the user provided function that contains the details of what to do with this object if a user calls <see cref="ASLObject.SendFloat4(float[])"/></param>
        private static void SendSpawnPrefab(string _prefabName, Vector3 _position, Quaternion _rotation, string _parentID = "", string _componentAssemblyQualifiedName = "", string _instantiatedGameObjectClassName = "", string _instantiatedGameObjectFunctionName = "",
            string _claimRecoveryClassName = "", string _claimRecoveryFunctionName = "", string _sendFloatClassName = "", string _sendFloatFunctionName = "")
        {
            string guid = Guid.NewGuid().ToString();
            using (RTData data = RTData.Get())
            {
                data.SetString((int)GameController.DataCode.Id, guid);
                data.SetString((int)GameController.DataCode.PrefabName, _prefabName);
                data.SetVector3((int)GameController.DataCode.Position, _position);
                data.SetVector4((int)GameController.DataCode.Rotation, new Vector4(_rotation.x, _rotation.y, _rotation.z, _rotation.w));
                data.SetString((int)GameController.DataCode.ParentId, _parentID);
                data.SetString((int)GameController.DataCode.ComponentName, _componentAssemblyQualifiedName);
                data.SetString((int)GameController.DataCode.InstantiatedGameObjectClassName, _instantiatedGameObjectClassName);
                data.SetString((int)GameController.DataCode.InstantiatedGameObjectFunctionName, _instantiatedGameObjectFunctionName);
                data.SetString((int)GameController.DataCode.ClaimRecoveryClassName, _claimRecoveryClassName);
                data.SetString((int)GameController.DataCode.ClaimRecoveryFunctionName, _claimRecoveryFunctionName);
                data.SetString((int)GameController.DataCode.SendFloatClassName, _sendFloatClassName);
                data.SetString((int)GameController.DataCode.SendFloatFunctionName, _sendFloatFunctionName);
                GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.SpawnPrefab, GameSparksRT.DeliveryIntent.RELIABLE, data);
            }
        }

        /// <summary>
        /// Change scene for all players. This function is called by a user. 
        /// </summary>
        /// <param name="_sceneName">The name of the scene to change to</param>
        /// <example><code>
        /// void SomeFunction()
        /// {
        ///     ASL.ASLHelper.SendAndSetNewScene("YourSceneName");
        /// }
        /// </code></example>
        public static void SendAndSetNewScene(string _sceneName)
        {
            using (RTData data = RTData.Get())
            {
                data.SetString((int)GameController.DataCode.SceneName, _sceneName);
                GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.LoadScene, GameSparksRT.DeliveryIntent.RELIABLE, data);
            }
        }

        /// <summary>
        /// Creates an ARCore Cloud Anchor at the location the user tapped and passed into it. This function can be used to set the world origin or just a normal cloud anchor.
        /// It is advisable to only have 1 user in your application set cloud anchors.
        /// </summary>
        /// <param name="_hitResults">Holds information about where the user tapped on the screen. This variable should be created using the ARWorldOriginHelper Raycast method </param>
        /// <param name="_anchorObjectPrefab">The ASL object you want to have located at the cloud anchor. If you don't want any object to be located at the cloud anchor, you can pass in null.
        /// Doing so will create an empty gameobject - thus making it invisible to users</param>
        /// <param name="_myPostCreateCloudAnchorFunction">This is the function you want to call after a cloud anchor has successfully been created. Only the user that called this function
        /// will execute this function - it is not sent to other users. This is a good way to move or create objects after a cloud anchor has been created.</param>
        /// <param name="_waitForAllUsersToResolve">This determines if users should wait to setup (and if they are the caller of this function, to execute the _myPostCreateCloudAnchorFunction)
        /// the cloud anchor once they receive and find it, or if they should wait for all users to receive and find it first before executing anything. The default is to wait for all users
        /// and is the suggested value as not waiting as the potential to cause synchronization problems.</param>
        /// <param name="_setWorldOrigin">This determines if this cloud anchor should be used to set the world origin for all users or not. If you are setting the world origin, you should do
        /// so right away in your app and as the first (if you have more than 1) cloud anchor created. You should never set the world origin more than once.</param>
        public static void CreateARCoreCloudAnchor(TrackableHit _hitResults, ASLObject _anchorObjectPrefab = null, ASLObject.PostCreateCloudAnchorFunction _myPostCreateCloudAnchorFunction = null, 
            bool _waitForAllUsersToResolve = true, bool _setWorldOrigin = true)
        {
            if (_anchorObjectPrefab != null) { if (!_anchorObjectPrefab.m_Mine) Debug.LogError("You must claim the ASL object before setting it as an anchor"); return; }
            //Create local anchor at hit location
            Anchor localAnchor = _hitResults.Trackable.CreateAnchor(_hitResults.Pose);
            localAnchor.name = "Local anchor created when creating cloud anchor";

            //Create CLoud anchor
            XPSession.CreateCloudAnchor(localAnchor).ThenAction(result =>
            {
                //If failed to host
                if (result.Response != CloudServiceResponse.Success) 
                {
                    Debug.LogError("Failed to host Cloud Anchor: " + result.Response);
                    return; //Break out
                }
                //Successful:
                Debug.Log("Successfully created and saved Cloud Anchor: " + result.Anchor.CloudId);

                if (_anchorObjectPrefab == null)
                {
                    //Uncomment the line below to aid in visual debugging (helps display the cloud anchor)
                    //_anchorObjectPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<ASLObject>(); //if null, then create empty game object               
                    _anchorObjectPrefab = new GameObject().AddComponent<ASLObject>();
                    _anchorObjectPrefab._LocallySetAnchorID(result.Anchor.CloudId); //Add ASLObject component to this anchor and set its anchor id variable
                    _anchorObjectPrefab._LocallySetID(result.Anchor.CloudId); //Locally set the id of this object to be that of the anchor id (which is unique)

                    //Add this anchor object to our ASL dictionary using the anchor id as its key. All users will do this once they resolve this cloud anchor to ensure they still in sync.
                    m_ASLObjects.Add(result.Anchor.CloudId, _anchorObjectPrefab.GetComponent<ASLObject>());
                    //_anchorObjectPrefab.GetComponent<Material>().color = Color.magenta;
                    _anchorObjectPrefab.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f); //Set scale to be 4 cm
                }
                else
                {
                    _anchorObjectPrefab.GetComponent<ASLObject>()._LocallySetAnchorID(result.Anchor.CloudId); //Set anchor id variable
                    _anchorObjectPrefab.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); //Set scale to be 5 cm
                }
              
                //Send Resolve packet using _anchorObjectPrefab 
                _anchorObjectPrefab.GetComponent<ASLObject>().SendCloudAnchorToResolve(_setWorldOrigin, _waitForAllUsersToResolve);

                if (_waitForAllUsersToResolve)
                {
                    //Send packet to relay server letting it know this user is ready
                    using (RTData data = RTData.Get())
                    {
                        data.SetString((int)GameController.DataCode.Id, _anchorObjectPrefab.m_Id);
                        GameSparksManager.Instance().GetRTSession().SendData((int)GameSparksManager.OpCode.ResolvedCloudAnchor, GameSparksRT.DeliveryIntent.RELIABLE, data);
                    }
                    _anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(result, _setWorldOrigin, _myPostCreateCloudAnchorFunction, _hitResults);
                }
                else //Don't wait for users to know about this cloud anchor
                {
                    _anchorObjectPrefab.GetComponent<ASLObject>()._LocallySetCloudAnchorResolved(true);
                    _anchorObjectPrefab.StartWaitForAllUsersToResolveCloudAnchor(result, _setWorldOrigin, _myPostCreateCloudAnchorFunction, _hitResults);
                }
            });

        }





    }
}
