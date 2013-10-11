using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Zen.Host
{
    public class AppHost
    {
        private readonly AppCore _core;
        private readonly List<IAppScope> _appScopeList;
        private readonly List<Task> _appTaskList=new List<Task>();
        private readonly Dictionary<IHostedApp,IAppScope> _hostedScopes=new Dictionary<IHostedApp, IAppScope>();
        private static readonly ILog Log = LogManager.GetLogger(typeof (AppHost));
        private readonly CancellationToken _cancellationToken;
        private readonly TaskFactory _factory;
        private readonly CancellationTokenSource _tokenSource;

        public AppHost(AppCore core)
        {
            _core = core;
            _appScopeList = new List<IAppScope>();
            _factory = new TaskFactory(_cancellationToken);
            _tokenSource = new CancellationTokenSource();
            _cancellationToken = _tokenSource.Token;
        }
        public void Start()
        {
            var appTypes = _core.Resolve<IEnumerable<IHostedApp>>();
            var appList = new List<IHostedApp>();
            foreach (var hostedAppType in appTypes.Select(a=>a.GetType()))
            {
                var scope = _core.BeginScope();
                _appScopeList.Add(scope);
                var app = (IHostedApp)scope.Resolve(hostedAppType);
                appList.Add(app);
                _hostedScopes[app] = scope;
                Log.InfoFormat("Запуск приложения {0}", hostedAppType);
                var task = _factory.StartNew((arg) =>
                    {
                        var curApp = (IHostedApp) arg;
                        try
                        {
                            curApp.Start();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Ошибка исполнения приложения " + curApp.GetType(), ex);
                        }
                        return curApp;
                    }, app).ContinueWith(t =>
                        {
                            Log.InfoFormat("Завершение приложения: " + t.Result.GetType());
                            var app1 = t.Result;
                            appList.Remove(app1);
                            var scope1 = _hostedScopes[app1];
                            _appScopeList.Remove(scope1);
                            _hostedScopes.Remove(app1);
                            scope1.Dispose();                            
                        });

                _appTaskList.Add(task);
                //task.Start();
            }

            Apps = appList;
        }
        public void Stop()
        {
            foreach (var hostedApp in Apps.ToArray())
            {
                Log.DebugFormat("Попытка завершения приложения: " + hostedApp.GetType());
                hostedApp.Stop();
                Log.InfoFormat("Завершено приложения: " + hostedApp.GetType());
            }
            foreach (var appScope in _appScopeList.ToArray())
            {
                appScope.Dispose();
            }

            _tokenSource.Cancel();
        }
        ~AppHost()
        {
            _core.Dispose();
        }
        public IEnumerable<IHostedApp> Apps { get; private set; }
    }
}
