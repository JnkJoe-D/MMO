#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System;


namespace XLua.CSObjectWrap
{
    public class SystemCollectionsGenericIList_1_BItemData_Bridge : LuaBase, System.Collections.Generic.IList<BItemData>
    {
	    public static LuaBase __Create(int reference, LuaEnv luaenv)
		{
		    return new SystemCollectionsGenericIList_1_BItemData_Bridge(reference, luaenv);
		}
		
		public SystemCollectionsGenericIList_1_BItemData_Bridge(int reference, LuaEnv luaenv) : base(reference, luaenv)
        {
        }
		
        
		int System.Collections.Generic.IList<BItemData>.IndexOf(BItemData item)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "IndexOf");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function IndexOf");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				translator.PushAny(L, item);
				
				int __gen_error = LuaAPI.lua_pcall(L, 2, 1, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				int __gen_ret = LuaAPI.xlua_tointeger(L, err_func + 1);
				LuaAPI.lua_settop(L, err_func - 1);
				return  __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		void System.Collections.Generic.IList<BItemData>.Insert(int index, BItemData item)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "Insert");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function Insert");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				LuaAPI.xlua_pushinteger(L, index);
				translator.PushAny(L, item);
				
				int __gen_error = LuaAPI.lua_pcall(L, 3, 0, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				
				LuaAPI.lua_settop(L, err_func - 1);
				
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		void System.Collections.Generic.IList<BItemData>.RemoveAt(int index)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "RemoveAt");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function RemoveAt");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				LuaAPI.xlua_pushinteger(L, index);
				
				int __gen_error = LuaAPI.lua_pcall(L, 2, 0, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				
				LuaAPI.lua_settop(L, err_func - 1);
				
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		System.Collections.Generic.IEnumerator<BItemData> System.Collections.Generic.IEnumerable<BItemData>.GetEnumerator()
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "GetEnumerator");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function GetEnumerator");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				
				int __gen_error = LuaAPI.lua_pcall(L, 1, 1, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				System.Collections.Generic.IEnumerator<BItemData> __gen_ret = (System.Collections.Generic.IEnumerator<BItemData>)translator.GetObject(L, err_func + 1, typeof(System.Collections.Generic.IEnumerator<BItemData>));
				LuaAPI.lua_settop(L, err_func - 1);
				return  __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "GetEnumerator");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function GetEnumerator");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				
				int __gen_error = LuaAPI.lua_pcall(L, 1, 1, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				System.Collections.IEnumerator __gen_ret = (System.Collections.IEnumerator)translator.GetObject(L, err_func + 1, typeof(System.Collections.IEnumerator));
				LuaAPI.lua_settop(L, err_func - 1);
				return  __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		void System.Collections.Generic.ICollection<BItemData>.Add(BItemData item)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "Add");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function Add");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				translator.PushAny(L, item);
				
				int __gen_error = LuaAPI.lua_pcall(L, 2, 0, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				
				LuaAPI.lua_settop(L, err_func - 1);
				
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		void System.Collections.Generic.ICollection<BItemData>.Clear()
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "Clear");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function Clear");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				
				int __gen_error = LuaAPI.lua_pcall(L, 1, 0, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				
				LuaAPI.lua_settop(L, err_func - 1);
				
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		bool System.Collections.Generic.ICollection<BItemData>.Contains(BItemData item)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "Contains");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function Contains");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				translator.PushAny(L, item);
				
				int __gen_error = LuaAPI.lua_pcall(L, 2, 1, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				bool __gen_ret = LuaAPI.lua_toboolean(L, err_func + 1);
				LuaAPI.lua_settop(L, err_func - 1);
				return  __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		void System.Collections.Generic.ICollection<BItemData>.CopyTo(BItemData[] array, int arrayIndex)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "CopyTo");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function CopyTo");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				translator.Push(L, array);
				LuaAPI.xlua_pushinteger(L, arrayIndex);
				
				int __gen_error = LuaAPI.lua_pcall(L, 3, 0, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				
				LuaAPI.lua_settop(L, err_func - 1);
				
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        
		bool System.Collections.Generic.ICollection<BItemData>.Remove(BItemData item)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
				RealStatePtr L = luaEnv.L;
				int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				ObjectTranslator translator = luaEnv.translator;
				
				LuaAPI.lua_getref(L, luaReference);
				LuaAPI.xlua_pushasciistring(L, "Remove");
				if (0 != LuaAPI.xlua_pgettable(L, -2))
				{
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				if(!LuaAPI.lua_isfunction(L, -1))
				{
					LuaAPI.xlua_pushasciistring(L, "no such function Remove");
					luaEnv.ThrowExceptionFromError(err_func - 1);
				}
				LuaAPI.lua_pushvalue(L, -2);
				LuaAPI.lua_remove(L, -3);
				translator.PushAny(L, item);
				
				int __gen_error = LuaAPI.lua_pcall(L, 2, 1, err_func);
				if (__gen_error != 0)
					luaEnv.ThrowExceptionFromError(err_func - 1);
				
				
				bool __gen_ret = LuaAPI.lua_toboolean(L, err_func + 1);
				LuaAPI.lua_settop(L, err_func - 1);
				return  __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
		}
        

        
        int System.Collections.Generic.ICollection<BItemData>.Count 
        {
            
            get 
            {
#if THREAD_SAFE || HOTFIX_ENABLE
                lock (luaEnv.luaEnvLock)
                {
#endif
					RealStatePtr L = luaEnv.L;
					int oldTop = LuaAPI.lua_gettop(L);
					
					LuaAPI.lua_getref(L, luaReference);
					LuaAPI.xlua_pushasciistring(L, "Count");
					if (0 != LuaAPI.xlua_pgettable(L, -2))
					{
						luaEnv.ThrowExceptionFromError(oldTop);
					}
					int __gen_ret = LuaAPI.xlua_tointeger(L, -1);
					LuaAPI.lua_pop(L, 2);
					return __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
                }
#endif
            }
            
            
        }
        
        bool System.Collections.Generic.ICollection<BItemData>.IsReadOnly 
        {
            
            get 
            {
#if THREAD_SAFE || HOTFIX_ENABLE
                lock (luaEnv.luaEnvLock)
                {
#endif
					RealStatePtr L = luaEnv.L;
					int oldTop = LuaAPI.lua_gettop(L);
					
					LuaAPI.lua_getref(L, luaReference);
					LuaAPI.xlua_pushasciistring(L, "IsReadOnly");
					if (0 != LuaAPI.xlua_pgettable(L, -2))
					{
						luaEnv.ThrowExceptionFromError(oldTop);
					}
					bool __gen_ret = LuaAPI.lua_toboolean(L, -1);
					LuaAPI.lua_pop(L, 2);
					return __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
                }
#endif
            }
            
            
        }
        
        
        
		
		
        BItemData System.Collections.Generic.IList<BItemData>.this[int index]
		{
		    get
			{
#if THREAD_SAFE || HOTFIX_ENABLE
				lock (luaEnv.luaEnvLock)
				{
#endif
					RealStatePtr L = luaEnv.L;
					int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
					ObjectTranslator translator = luaEnv.translator;
				
					LuaAPI.lua_getref(L, luaReference);
					LuaAPI.xlua_pushasciistring(L, "get_Item");
					if (0 != LuaAPI.xlua_pgettable(L, -2))
					{
						luaEnv.ThrowExceptionFromError(err_func - 1);
					}
					if(!LuaAPI.lua_isfunction(L, -1))
					{
						LuaAPI.xlua_pushasciistring(L, "no such function get_Item");
						luaEnv.ThrowExceptionFromError(err_func - 1);
					}
					LuaAPI.lua_pushvalue(L, -2);
					LuaAPI.lua_remove(L, -3);
					LuaAPI.xlua_pushinteger(L, index);
					
					int __gen_error = LuaAPI.lua_pcall(L, 2, 1, err_func);
					if (__gen_error != 0)
						luaEnv.ThrowExceptionFromError(err_func - 1);
				
					
					BItemData __gen_ret = (BItemData)translator.GetObject(L, err_func + 1, typeof(BItemData));
					LuaAPI.lua_settop(L, err_func - 1);
					return  __gen_ret;
#if THREAD_SAFE || HOTFIX_ENABLE
				}
#endif
			}
		
		
			set
			{
#if THREAD_SAFE || HOTFIX_ENABLE
				lock (luaEnv.luaEnvLock)
				{
#endif
					RealStatePtr L = luaEnv.L;
					int err_func = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
					ObjectTranslator translator = luaEnv.translator;
				
					LuaAPI.lua_getref(L, luaReference);
					LuaAPI.xlua_pushasciistring(L, "set_Item");
					if (0 != LuaAPI.xlua_pgettable(L, -2))
					{
						luaEnv.ThrowExceptionFromError(err_func - 1);
					}
					if(!LuaAPI.lua_isfunction(L, -1))
					{
						LuaAPI.xlua_pushasciistring(L, "no such function set_Item");
						luaEnv.ThrowExceptionFromError(err_func - 1);
					}
					LuaAPI.lua_pushvalue(L, -2);
					LuaAPI.lua_remove(L, -3);
					LuaAPI.xlua_pushinteger(L, index);
					translator.PushAny(L, value);
					
					int __gen_error = LuaAPI.lua_pcall(L, 3, 0, err_func);
					if (__gen_error != 0)
						luaEnv.ThrowExceptionFromError(err_func - 1);
				
					
					
					LuaAPI.lua_settop(L, err_func - 1);
					
#if THREAD_SAFE || HOTFIX_ENABLE
				}
#endif
			} 
		
		}
        
	}
}
