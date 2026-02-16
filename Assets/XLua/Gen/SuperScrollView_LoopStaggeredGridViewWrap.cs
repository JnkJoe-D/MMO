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
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class SuperScrollViewLoopStaggeredGridViewWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(SuperScrollView.LoopStaggeredGridView);
			Utils.BeginObjectRegister(type, L, translator, 0, 26, 17, 4);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetItemGroupByIndex", _m_GetItemGroupByIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetItemPrefabConfData", _m_GetItemPrefabConfData);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "InitListView", _m_InitListView);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ResetGridViewLayoutParam", _m_ResetGridViewLayoutParam);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "NewListViewItem", _m_NewListViewItem);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetListItemCount", _m_SetListItemCount);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "MovePanelToItemIndex", _m_MovePanelToItemIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetShownItemByItemIndex", _m_GetShownItemByItemIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RefreshAllShownItem", _m_RefreshAllShownItem);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnItemSizeChanged", _m_OnItemSizeChanged);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RefreshItemByItemIndex", _m_RefreshItemByItemIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ResetListView", _m_ResetListView);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RecycleAllItem", _m_RecycleAllItem);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RecycleItemTmp", _m_RecycleItemTmp);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ClearAllTmpRecycledItem", _m_ClearAllTmpRecycledItem);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnBeginDrag", _m_OnBeginDrag);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnEndDrag", _m_OnEndDrag);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "OnDrag", _m_OnDrag);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetItemIndexData", _m_GetItemIndexData);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UpdateAllGroupShownItemsPos", _m_UpdateAllGroupShownItemsPos);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetItemAbsPosByItemIndex", _m_GetItemAbsPosByItemIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetNewItemByGroupAndIndex", _m_GetNewItemByGroupAndIndex);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UpdateListViewWithDefault", _m_UpdateListViewWithDefault);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UpdateListView", _m_UpdateListView);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetContentSize", _m_GetContentSize);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UpdateContentSize", _m_UpdateContentSize);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "ArrangeType", _g_get_ArrangeType);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemPrefabDataList", _g_get_ItemPrefabDataList);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ListUpdateCheckFrameCount", _g_get_ListUpdateCheckFrameCount);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IsVertList", _g_get_IsVertList);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ItemTotalCount", _g_get_ItemTotalCount);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ContainerTrans", _g_get_ContainerTrans);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ScrollRect", _g_get_ScrollRect);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IsDraging", _g_get_IsDraging);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "LayoutParam", _g_get_LayoutParam);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "IsInited", _g_get_IsInited);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ViewPortSize", _g_get_ViewPortSize);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ViewPortWidth", _g_get_ViewPortWidth);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "ViewPortHeight", _g_get_ViewPortHeight);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "CurMaxCreatedItemIndexCount", _g_get_CurMaxCreatedItemIndexCount);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "mOnBeginDragAction", _g_get_mOnBeginDragAction);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "mOnDragingAction", _g_get_mOnDragingAction);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "mOnEndDragAction", _g_get_mOnEndDragAction);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "ArrangeType", _s_set_ArrangeType);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "mOnBeginDragAction", _s_set_mOnBeginDragAction);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "mOnDragingAction", _s_set_mOnDragingAction);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "mOnEndDragAction", _s_set_mOnEndDragAction);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new SuperScrollView.LoopStaggeredGridView();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to SuperScrollView.LoopStaggeredGridView constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemGroupByIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _index = LuaAPI.xlua_tointeger(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.GetItemGroupByIndex( _index );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemPrefabConfData(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _prefabName = LuaAPI.lua_tostring(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.GetItemPrefabConfData( _prefabName );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_InitListView(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 5&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& translator.Assignable<SuperScrollView.GridViewLayoutParam>(L, 3)&& translator.Assignable<System.Func<SuperScrollView.LoopStaggeredGridView, int, SuperScrollView.LoopStaggeredGridViewItem>>(L, 4)&& translator.Assignable<SuperScrollView.StaggeredGridViewInitParam>(L, 5)) 
                {
                    int _itemTotalCount = LuaAPI.xlua_tointeger(L, 2);
                    SuperScrollView.GridViewLayoutParam _layoutParam = (SuperScrollView.GridViewLayoutParam)translator.GetObject(L, 3, typeof(SuperScrollView.GridViewLayoutParam));
                    System.Func<SuperScrollView.LoopStaggeredGridView, int, SuperScrollView.LoopStaggeredGridViewItem> _onGetItemByItemIndex = translator.GetDelegate<System.Func<SuperScrollView.LoopStaggeredGridView, int, SuperScrollView.LoopStaggeredGridViewItem>>(L, 4);
                    SuperScrollView.StaggeredGridViewInitParam _initParam = (SuperScrollView.StaggeredGridViewInitParam)translator.GetObject(L, 5, typeof(SuperScrollView.StaggeredGridViewInitParam));
                    
                    gen_to_be_invoked.InitListView( _itemTotalCount, _layoutParam, _onGetItemByItemIndex, _initParam );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& translator.Assignable<SuperScrollView.GridViewLayoutParam>(L, 3)&& translator.Assignable<System.Func<SuperScrollView.LoopStaggeredGridView, int, SuperScrollView.LoopStaggeredGridViewItem>>(L, 4)) 
                {
                    int _itemTotalCount = LuaAPI.xlua_tointeger(L, 2);
                    SuperScrollView.GridViewLayoutParam _layoutParam = (SuperScrollView.GridViewLayoutParam)translator.GetObject(L, 3, typeof(SuperScrollView.GridViewLayoutParam));
                    System.Func<SuperScrollView.LoopStaggeredGridView, int, SuperScrollView.LoopStaggeredGridViewItem> _onGetItemByItemIndex = translator.GetDelegate<System.Func<SuperScrollView.LoopStaggeredGridView, int, SuperScrollView.LoopStaggeredGridViewItem>>(L, 4);
                    
                    gen_to_be_invoked.InitListView( _itemTotalCount, _layoutParam, _onGetItemByItemIndex );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to SuperScrollView.LoopStaggeredGridView.InitListView!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ResetGridViewLayoutParam(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _itemTotalCount = LuaAPI.xlua_tointeger(L, 2);
                    SuperScrollView.GridViewLayoutParam _layoutParam = (SuperScrollView.GridViewLayoutParam)translator.GetObject(L, 3, typeof(SuperScrollView.GridViewLayoutParam));
                    
                    gen_to_be_invoked.ResetGridViewLayoutParam( _itemTotalCount, _layoutParam );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_NewListViewItem(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _itemPrefabName = LuaAPI.lua_tostring(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.NewListViewItem( _itemPrefabName );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetListItemCount(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 3)) 
                {
                    int _itemCount = LuaAPI.xlua_tointeger(L, 2);
                    bool _resetPos = LuaAPI.lua_toboolean(L, 3);
                    
                    gen_to_be_invoked.SetListItemCount( _itemCount, _resetPos );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2)) 
                {
                    int _itemCount = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.SetListItemCount( _itemCount );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to SuperScrollView.LoopStaggeredGridView.SetListItemCount!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MovePanelToItemIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _itemIndex = LuaAPI.xlua_tointeger(L, 2);
                    float _offset = (float)LuaAPI.lua_tonumber(L, 3);
                    
                    gen_to_be_invoked.MovePanelToItemIndex( _itemIndex, _offset );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetShownItemByItemIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _itemIndex = LuaAPI.xlua_tointeger(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.GetShownItemByItemIndex( _itemIndex );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RefreshAllShownItem(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.RefreshAllShownItem(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnItemSizeChanged(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _itemIndex = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.OnItemSizeChanged( _itemIndex );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RefreshItemByItemIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _itemIndex = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.RefreshItemByItemIndex( _itemIndex );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ResetListView(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _resetPos = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.ResetListView( _resetPos );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.ResetListView(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to SuperScrollView.LoopStaggeredGridView.ResetListView!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecycleAllItem(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.RecycleAllItem(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RecycleItemTmp(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    SuperScrollView.LoopStaggeredGridViewItem _item = (SuperScrollView.LoopStaggeredGridViewItem)translator.GetObject(L, 2, typeof(SuperScrollView.LoopStaggeredGridViewItem));
                    
                    gen_to_be_invoked.RecycleItemTmp( _item );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ClearAllTmpRecycledItem(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.ClearAllTmpRecycledItem(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnBeginDrag(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.EventSystems.PointerEventData _eventData = (UnityEngine.EventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.EventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnBeginDrag( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnEndDrag(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.EventSystems.PointerEventData _eventData = (UnityEngine.EventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.EventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnEndDrag( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_OnDrag(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.EventSystems.PointerEventData _eventData = (UnityEngine.EventSystems.PointerEventData)translator.GetObject(L, 2, typeof(UnityEngine.EventSystems.PointerEventData));
                    
                    gen_to_be_invoked.OnDrag( _eventData );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemIndexData(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _itemIndex = LuaAPI.xlua_tointeger(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.GetItemIndexData( _itemIndex );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UpdateAllGroupShownItemsPos(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.UpdateAllGroupShownItemsPos(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetItemAbsPosByItemIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _itemIndex = LuaAPI.xlua_tointeger(L, 2);
                    
                        var gen_ret = gen_to_be_invoked.GetItemAbsPosByItemIndex( _itemIndex );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetNewItemByGroupAndIndex(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _groupIndex = LuaAPI.xlua_tointeger(L, 2);
                    int _indexInGroup = LuaAPI.xlua_tointeger(L, 3);
                    
                        var gen_ret = gen_to_be_invoked.GetNewItemByGroupAndIndex( _groupIndex, _indexInGroup );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UpdateListViewWithDefault(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.UpdateListViewWithDefault(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UpdateListView(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _distanceForRecycle0 = (float)LuaAPI.lua_tonumber(L, 2);
                    float _distanceForRecycle1 = (float)LuaAPI.lua_tonumber(L, 3);
                    float _distanceForNew0 = (float)LuaAPI.lua_tonumber(L, 4);
                    float _distanceForNew1 = (float)LuaAPI.lua_tonumber(L, 5);
                    
                    gen_to_be_invoked.UpdateListView( _distanceForRecycle0, _distanceForRecycle1, _distanceForNew0, _distanceForNew1 );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetContentSize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        var gen_ret = gen_to_be_invoked.GetContentSize(  );
                        LuaAPI.lua_pushnumber(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UpdateContentSize(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.UpdateContentSize(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ArrangeType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ArrangeType);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemPrefabDataList(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ItemPrefabDataList);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ListUpdateCheckFrameCount(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ListUpdateCheckFrameCount);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IsVertList(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IsVertList);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ItemTotalCount(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.ItemTotalCount);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ContainerTrans(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ContainerTrans);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ScrollRect(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.ScrollRect);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IsDraging(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IsDraging);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_LayoutParam(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.LayoutParam);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_IsInited(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.IsInited);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ViewPortSize(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.ViewPortSize);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ViewPortWidth(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.ViewPortWidth);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_ViewPortHeight(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.ViewPortHeight);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_CurMaxCreatedItemIndexCount(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.CurMaxCreatedItemIndexCount);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mOnBeginDragAction(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.mOnBeginDragAction);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mOnDragingAction(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.mOnDragingAction);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mOnEndDragAction(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.mOnEndDragAction);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_ArrangeType(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                SuperScrollView.ListItemArrangeType gen_value;translator.Get(L, 2, out gen_value);
				gen_to_be_invoked.ArrangeType = gen_value;
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mOnBeginDragAction(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mOnBeginDragAction = translator.GetDelegate<System.Action>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mOnDragingAction(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mOnDragingAction = translator.GetDelegate<System.Action>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mOnEndDragAction(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                SuperScrollView.LoopStaggeredGridView gen_to_be_invoked = (SuperScrollView.LoopStaggeredGridView)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mOnEndDragAction = translator.GetDelegate<System.Action>(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
