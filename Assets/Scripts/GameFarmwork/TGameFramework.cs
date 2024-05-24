using System;
using System.Collections.Generic;

public sealed class TGameFramework
{
    public static TGameFramework Instance { get; private set; }
    public static bool Initialized { get; private set; }
    //private List<BaseGameModule> s_modules;
    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>();

    public static void Initialize()
    {
        Instance = new TGameFramework();
    }

    public T GetModule<T>() where T : BaseGameModule
    {
        if (m_modules.TryGetValue(typeof(T), out BaseGameModule module))
        {
            return module as T;
        }

        return default(T);
    }

    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();
        if (m_modules.ContainsKey(moduleType))
        {
            UnityLog.Info($"Module添加失败，重复:{moduleType.Name}");
            return;
        }
        m_modules.Add(moduleType, module);
    }

    //        private IEnumerator Start()
    //        {
    //            if (Instance != null)
    //            {
    //                gameObject.SetActive(false);
    //                yield break;
    //                //return;
    //
    //            UnityLog.Info("===游戏启动===");
    //            Instance = this;
    //            DontDestroyOnLoad(gameObject);
    //            UnityLog.Info("===>加载配置");
    //#if UNITY_EDITOR
    //            string path = "Assets/Datas/Config";
    //            ConfigManager.LoadAllConfigsByFile(path);
    //            //yield return ConfigManager.LoadAllConfigsByFile(path);
    //#else
    //            string path = $"{Application.streamingAssetsPath}/AssetBundles";
    //            string subFolder = $"Datas/Config";
    //            //ConfigManager.LoadAllConfigsByBundle(path, subFolder);
    //            yield return ConfigManager.LoadAllConfigsByBundle(path, subFolder);
    //#endif
    //            GlobalConfig.InitGlobalConfig();
    //            BuffConfig.ParseConfig();
    //            SkillConfig.ParseConfig();
    //            SkillRuneConfig.ParseConfig();
    //            BulletConfig.ParseConfig();
    //            SpellFieldConfig.ParseConfig();
    //            LevelConfig.ParseConfig();
    //            WeaponConfig.ParseConfig();
    //            DialogConfig.ParseConfig();
    //            BlessingConfig.ParseConfig();
    //            SpawnMonsterConfig.ParseConfig();
    //            I18NConfig.ParseConfig();
    //            GameEffectHandler.RegisterHandlers();
    //            UnityLog.Info("<===配置加载完毕");
    //
    //            UnityLog.Info("===>初始化模块组件");
    //            Instance.InitModules();
    //            UnityLog.Info("<===模块组件初始化完毕");
    //            UnityLog.Info("===>启动模块组件");
    //            Instance.StartModules();
    //            UnityLog.Info("<===模块组件已就绪");
    //        }

    public void Update()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleUpdate(deltaTime);
        }
    }

    public void LateUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.deltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleLateUpdate(deltaTime);
        }
    }

    public void FixedUpdate()
    {
        if (!Initialized)
            return;

        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        float deltaTime = UnityEngine.Time.fixedDeltaTime;
        foreach (var module in m_modules.Values)
        {
            module.OnModuleFixedUpdate(deltaTime);
        }
    }

    public void InitModules()
    {
        if (Initialized)
            return;

        Initialized = true;
        //StartupModules();
        foreach (var module in m_modules.Values)
        {
            module.OnModuleInit();
        }
    }

    public void StartModules()
    {
        if (m_modules == null)
            return;

        if (!Initialized)
            return;

        foreach (var module in m_modules.Values)
        {
            module.OnModuleStart();
        }
    }

    public void Destroy()
    {
        if (!Initialized)
            return;

        if (Instance != this)
            return;

        if (Instance.m_modules == null)
            return;

        foreach (var module in Instance.m_modules.Values)
        {
            module.OnModuleStop();
        }

        //Destroy(Instance.gameObject);
        Instance = null;
        Initialized = false;
    }
}



