﻿using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebApiClient.Contexts;

namespace WebApiClient
{
    /// <summary>
    /// 表示Api的拦截器
    /// </summary>
    class ApiInterceptor : IInterceptor
    {
        /// <summary>
        /// httpApi配置
        /// </summary>
        private readonly HttpApiConfig httpApiConfig;

        /// <summary>
        /// dispose方法
        /// </summary>
        private static readonly MethodInfo disposeMethod = typeof(IDisposable).GetMethods().FirstOrDefault();

        /// <summary>
        /// Api的拦截器
        /// </summary>
        /// <param name="apiConfig">httpApi配置</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ApiInterceptor(HttpApiConfig apiConfig)
        {
            if (apiConfig == null)
            {
                throw new ArgumentNullException();
            }

            apiConfig.SetNullPropertyAsDefault();
            this.httpApiConfig = apiConfig;
        }

        /// <summary>
        /// 拦截方法的调用
        /// </summary>
        /// <param name="invocation">方法上下文</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Equals(disposeMethod) == true)
            {
                this.httpApiConfig.Dispose();
            }
            else
            {
                invocation.ReturnValue = this.ExecuteHttpApi(invocation);
            }
        }

        /// <summary>
        /// 执行Http请求接口
        /// </summary>
        /// <param name="invocation">上下文</param>
        /// <returns></returns>
        private Task ExecuteHttpApi(IInvocation invocation)
        {
            var cache = ApiDescriptorCache.GetApiActionDescriptor(invocation);
            var actionDescripter = cache.Clone() as ApiActionDescriptor;

            for (var i = 0; i < actionDescripter.Parameters.Length; i++)
            {
                actionDescripter.Parameters[i].Value = invocation.GetArgumentValue(i);
            }

            var actionContext = new ApiActionContext
            {
                ApiActionDescriptor = actionDescripter,
                HttpApiConfig = this.httpApiConfig,
                RequestMessage = new HttpRequestMessage { RequestUri = this.httpApiConfig.HttpHost },
                ResponseMessage = null
            };

            return actionDescripter.Execute(actionContext);
        }
    }
}
