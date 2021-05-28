//using System;

//namespace IChangeToken.Application
//{
//    public interface IChangeToken
//    {
//        /// <summary>
//        /// 判断当前token是否改变
//        /// </summary>
//        bool HasChanged { get; }

//        /// <summary>
//        /// token自动触发回调,token消费者自动接收到改变
//        /// poll <see cref="HasChanged" /> to detect changes.
//        /// </summary>
//        bool ActiveChangeCallbacks { get; }

//        /// <summary>
//        /// 注册回调函数,当实体发现改变的时候
//        /// <see cref="HasChanged"/> 回调触发前,必须设置HasChanged值
//        /// </summary>
//        /// <param name="callback">The <see cref="Action{Object}"/> to invoke.</param>
//        /// <param name="state">State to be passed into the callback.</param>
//        /// <returns>An <see cref="IDisposable"/> that is used to unregister the callback.</returns>
//        IDisposable RegisterChangeCallback(Action<object> callback, object state);
//    }
//}
