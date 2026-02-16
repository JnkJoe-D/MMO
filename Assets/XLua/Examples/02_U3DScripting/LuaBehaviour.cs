/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using UnityEditor;
using UnityEngine.UI;

namespace XLuaTest
{
    /// <summary>
    /// 注入类，将Unity对象传递给Lua脚本
    /// </summary>
    [System.Serializable]
    public class Injection
    {
        public string name;
        public GameObject value;
    }

    [LuaCallCSharp]
    public class LuaBehaviour : MonoBehaviour
    {
        // Lua脚本资源
        public TextAsset luaScript;
        // 需要注入的Unity对象列表
        public Injection[] injections;

        // 所有LuaBehaviour共享同一个LuaEnv
        internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
        //上次GC时间
        internal static float lastGCTime = 0;
        //GC间隔时间
        internal const float GCInterval = 1;//1 second 
        private const string LuaPath = "Assets/LuaScript/";

        //对应Lua中的start函数
        private Action luaStart;
        //对应Lua中的update函数
        private Action luaUpdate;
        //对应Lua中的ondestroy函数
        private Action luaOnDestroy;
        // 每个Lua脚本的独立作用域表
        private LuaTable scriptScopeTable;

        void Awake()
        {
            luaEnv.AddLoader(CustomLoader);
            // 为每个脚本设置一个独立的脚本域，可一定程度上防止脚本间全局变量、函数冲突
            scriptScopeTable = luaEnv.NewTable();

            // 设置其元表的 __index, 使其能够访问全局变量
            using (LuaTable meta = luaEnv.NewTable())
            {
                meta.Set("__index", luaEnv.Global);
                scriptScopeTable.SetMetaTable(meta);
            }

            // 将所需值注入到 Lua 脚本域中,即_Env.self = this
            scriptScopeTable.Set("self", this);
            foreach (var injection in injections)
            {
                scriptScopeTable.Set(injection.name, injection.value);
            }

            // 如果你希望在脚本内能够设置全局变量, 也可以直接将全局脚本域注入到当前脚本的脚本域中
            // 这样, 你就可以在 Lua 脚本中通过 Global.XXX 来访问全局变量
            // scriptScopeTable.Set("Global", luaEnv.Global);

            // 执行脚本
            // luaEnv.DoString(luaScript.text, luaScript.name, scriptScopeTable);
            luaEnv.DoString(@"require('GameUi')", "GameUi", scriptScopeTable);

            // 从 Lua 脚本域中获取定义的函数
            Action luaAwake = scriptScopeTable.Get<Action>("awake");
            scriptScopeTable.Get("start", out luaStart);
            scriptScopeTable.Get("update", out luaUpdate);
            scriptScopeTable.Get("ondestroy", out luaOnDestroy);

            if (luaAwake != null)
            {
                luaAwake();
            }
        }

        // Use this for initialization
        void Start()
        {
            if (luaStart != null)
            {
                luaStart();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (luaUpdate != null)
            {
                luaUpdate();
            }

            if (Time.time - LuaBehaviour.lastGCTime > GCInterval)
            {
                luaEnv.Tick();
                LuaBehaviour.lastGCTime = Time.time;
            }
        }

        void OnDestroy()
        {
            if (luaOnDestroy != null)
            {
                luaOnDestroy();
            }

            scriptScopeTable.Dispose();
            luaOnDestroy = null;
            luaUpdate = null;
            luaStart = null;
            injections = null;
        }
        public byte[] LoadScript(string pathFileName)
        {
            pathFileName = string.Concat(LuaPath, pathFileName);
            pathFileName = string.Concat(pathFileName,".txt");
            //Assets / LuaScript / Test1Logic.txt
            TextAsset textAsset = ResManager.Load<TextAsset>(pathFileName);
            if (textAsset != null)
            {
                byte[] bytes = textAsset.bytes;

                return bytes;
            }

            return null;
        }

        private byte[] CustomLoader(ref string pathFileName)
        {
            return LoadScript(pathFileName);
        }
    }
}
