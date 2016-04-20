﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using IAsyncResult = Crestron.SimplSharp.CrestronIO.IAsyncResult;
using AsyncCallback = Crestron.SimplSharp.CrestronIO.AsyncCallback;

namespace Crestron.SimplSharp
	{
	public static class DelegateExtensions
		{
		/// <summary>
		/// Dynamically invokes (late-bound) the method represented by the current
		/// delegate. 
		/// </summary>
		/// <param name="dlg">Extended class.</param>
		/// <param name="args">An array of objects that are the arguments to pass to the
		/// method represented by the current delegate.</param>
		/// <returns>The object returned by the method represented by the delegate.
		/// </returns>
		/// <exception cref="MemberAccessException"> The caller does not have access to the
		/// method represented by the delegate (for example, if the method is private); The
		/// number, order, or type of parameters listed in <paramref name="args"/> is invalid. </exception>
		/// <exception cref="TargetInvocationException">One of the encapsulated methods
		/// throws an exception.</exception>
		public static object DynamicInvoke (this Delegate dlg, params object[] args)
			{
			return dlg.GetMethod().Invoke (dlg.Target, BindingFlags.Default, null, args, null);
			}

		/// <summary>
		/// Dynamically invokes (late-bound) the method represented by the current
		/// delegate. 
		/// </summary>
		/// <param name="dlg">Extended class.</param>
		/// <param name="args">An array of objects that are the arguments to pass to the
		/// method represented by the current delegate.</param>
		/// <returns>The object returned by the method represented by the delegate.
		/// </returns>
		/// <exception cref="MemberAccessException"> The caller does not have access to the
		/// method represented by the delegate (for example, if the method is private); The
		/// number, order, or type of parameters listed in <paramref name="args"/> is invalid. </exception>
		/// <exception cref="TargetInvocationException">One of the encapsulated methods
		/// throws an exception.</exception>
		public static TResult DynamicInvoke<TResult> (this Delegate dlg, params object[] args)
			{
			return (TResult)dlg.GetMethod().Invoke (dlg.Target, BindingFlags.Default, null, args, null);
			}

		private class InvokeInfo
			{
			public AsyncResult result;
			public AsyncCallback callback;
			public object state;
			public object[] args;
			}

		public static IAsyncResult BeginInvokeEx (this Delegate dlg, AsyncCallback callback, object obj, params object[] args)
			{
			var iar = new AsyncResult (dlg, obj);
			var invokeInfo = new InvokeInfo {result = iar, callback = callback, state = obj, args = args};
			CrestronInvoke.BeginInvoke (DoDelegate, invokeInfo);
			return iar;
			}

		private static void DoDelegate (object state)
			{
			var invokeInfo = (InvokeInfo)state;

			invokeInfo.result.Result = invokeInfo.result.AsyncDelegate.DynamicInvoke (invokeInfo.args);

			invokeInfo.result.IsCompleted = true;
			((CEvent)invokeInfo.result.AsyncWaitHandle).Set ();
			if (invokeInfo.callback != null)
				invokeInfo.callback (invokeInfo.result);
			}

		public static object EndInvokeEx (this Delegate dlg, IAsyncResult result)
			{
			var asyncResult = result as AsyncResult;
			if (asyncResult == null)
				throw new ArgumentException ("invalid IAsyncResult", "result");

			if (asyncResult.EndInvokeCalled)
				throw new InvalidOperationException ("EndInvoke can only be called onece");
			asyncResult.EndInvokeCalled = true;

			if (!asyncResult.CompletedSynchronously)
				asyncResult.AsyncWaitHandle.Wait ();

			return asyncResult.Result;
			}

		public static TResult EndInvokeEx<TResult> (this Delegate dlg, IAsyncResult result)
			{
			return (TResult)dlg.EndInvokeEx (result);
			}
		}
	}