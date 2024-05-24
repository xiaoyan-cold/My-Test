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
    /// ��ʼ��ģ��
    /// </summary>
    public void StartupModules()
    {
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        // ��ȡ��ǰ����GameManager�����ͣ�ͨ�������ȡ���е�������Ϣ�������ģ�˽�еģ��Լ���̬��
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        // ��ȡBaseGameModule������
        Type baseCompType = typeof(BaseGameModule);

        for (int i = 0; i < propertyInfos.Length; i++)
        {
            // ȡ������
            PropertyInfo property = propertyInfos[i];
            // �����Լ�飬IsAssignableFrom �� C# �� Type ���ʵ�������������ж�һ�������Ƿ���Դ���һ�����ͽ��и�ֵ��
            // c ��Ҫ���и�ֵ�������͡��÷�������һ������ֵ����ʾ���÷����������Ƿ���Դ� c ���ͽ��и�ֵ������ֵΪ true ��ʾ���Ը�ֵ������ֵΪ false ��ʾ�����Ը�ֵ��
            // �����������;�Ǽ����������֮��ļ����ԡ����һ�����Ϳ��Դ���һ�����ͽ��и�ֵ����ʾ����֮����ڼ��ݹ�ϵ�����ּ����Թ�ϵͨ����ָ�̳й�ϵ�����������Ϳ��Ը�ֵ�������͡�
            // property.PropertyType��ȡ�����Ե���������
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
            // ������Ե�����û�м̳�BaseGameModule,��������ѭ��
                continue;

            // ���ڴ�һ�����ԣ�property���л�ȡӦ���ڸ����Ե��ض��Զ������ԣ�attribute��
            // GetCustomAttributes(Type attributeType, bool inherit): ���Ƿ����е�һ�����������ڻ�ȡӦ�������ԡ���������ȳ�Ա���Զ������ԡ�
            // attributeType: ָ��Ҫ���������Ե����ͣ���������ModuleAttribute��
            // inherit: һ������ֵ��ָ���Ƿ�Ҳ�����̳����Բ�����Щ���ԡ�false��ʾ�������̳�����
            // object[] attrs: ����һ��object���飬���а���������Ӧ����property��ModuleAttributeʵ����
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            Debug.Log(attrs[0]);
            // Ӧ����property��ModuleAttributeʵ����
            if (attrs.Length == 0)
                continue;
            // ��ȡproperty.PropertyType���͵��Ӷ���
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
    /// �����������ƣ�ֻ���������ԣ����ܱ�����̳У�������ͬһ����Ԫ����Ӧ�ö��ʵ��
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>    // �Զ�������
    {
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// ģ��
        /// </summary>
        public BaseGameModule Module { get; set; }

        /// <summary>
        /// ���캯���������ȼ���ֵ
        /// </summary>
        /// <param name="priority"></param>
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }
        // �������ʵ����`IComparable<ModuleAttribute>`�ӿڣ����ڱȽ�����`ModuleAttribute`ʵ�������ȼ���
        // CompareTo��������һ��������
        // С��0����ʾ��ǰʵ�������ȼ�С��`other`ʵ�������ȼ��ߣ���
        // ����0����ʾ����ʵ�������ȼ���ͬ��
        // ����0����ʾ��ǰʵ�������ȼ�����`other`ʵ�������ȼ��ͣ���
        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}

