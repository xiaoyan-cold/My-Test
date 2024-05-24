using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class GameManager : MonoBehaviour
{

    [Module(6)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }

    private bool activing;
    private void Awake()
    {
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        activing = true;
        DontDestroyOnLoad(gameObject);
#if UNITY_EDITOR
        UnityLog.StartupEditor();
#else
            UnityLog.Startup();
#endif

        Application.logMessageReceived += OnReceiveLog;
        TGameFramework.Initialize();
        StartupModules();
        TGameFramework.Instance.InitModules();

    }

    private void Start()
    {
        TGameFramework.Instance.StartModules();
        //Procedure.StartProcedure().Coroutine();
    }

    private void Update()
    {
        TGameFramework.Instance.Update();
    }

    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate();
    }

    private void OnDestroy()
    {
        if (activing)
        {
            Application.logMessageReceived -= OnReceiveLog;
            TGameFramework.Instance.Destroy();
        }
    }
    private void OnReceiveLog(string condition, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
            if (type == LogType.Exception)
            {
                UnityLog.Fatal($"{condition}\n{stackTrace}");
            }
#endif
    }
    /// <summary>
    /// 初始化模块
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        // 获取当前对象GameManager的类型，通过反射获取所有的属性信息，公开的，私有的，以及静态的
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        // 获取BaseGameModule的类型
        Type baseCompType = typeof(BaseGameModule);

        for (int i = 0; i < propertyInfos.Length; i++)
        {
            // 取出属性
            PropertyInfo property = propertyInfos[i];
            // 兼容性检查，IsAssignableFrom 是 C# 中 Type 类的实例方法，用于判断一个类型是否可以从另一个类型进行赋值。
            // c 是要进行赋值检查的类型。该方法返回一个布尔值，表示调用方法的类型是否可以从 c 类型进行赋值。返回值为 true 表示可以赋值，返回值为 false 表示不可以赋值。
            // 这个方法的用途是检查两个类型之间的兼容性。如果一个类型可以从另一个类型进行赋值，表示它们之间存在兼容关系。这种兼容性关系通常是指继承关系，即派生类型可以赋值给基类型。
            // property.PropertyType获取该属性的数据类型
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
            // 如果属性的类型没有继承BaseGameModule,跳出本次循环
                continue;

            // 用于从一个属性（property）中获取应用于该属性的特定自定义特性（attribute）
            // GetCustomAttributes(Type attributeType, bool inherit): 这是反射中的一个方法，用于获取应用于属性、方法、类等成员的自定义特性。
            // attributeType: 指定要搜索的特性的类型，在这里是ModuleAttribute。
            // inherit: 一个布尔值，指定是否也搜索继承链以查找这些特性。false表示不搜索继承链。
            // object[] attrs: 返回一个object数组，其中包含了所有应用于property的ModuleAttribute实例。
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            Debug.Log(attrs[0]);
            // 应用于property的ModuleAttribute实例。
            if (attrs.Length == 0)
                continue;
            // 获取property.PropertyType类型的子对象
            Component comp = GetComponentInChildren(property.PropertyType);
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }

            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;

            moduleAttrs.Add(moduleAttr);
        }

        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;
        });

        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
    }

    /// <summary>
    /// 进行特性限制，只能用于属性，不能被子类继承，不允许同一程序元素上应用多个实例
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>    // 自定义特性
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }

        /// <summary>
        /// 构造函数，给优先级赋值
        /// </summary>
        /// <param name="priority"></param>
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }
        // 这个方法实现了`IComparable<ModuleAttribute>`接口，用于比较两个`ModuleAttribute`实例的优先级。
        // CompareTo方法返回一个整数：
        // 小于0，表示当前实例的优先级小于`other`实例（优先级高）。
        // 等于0，表示两个实例的优先级相同。
        // 大于0，表示当前实例的优先级大于`other`实例（优先级低）。
        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}

