using System.ServiceProcess;

namespace ErrorFileWatcher
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            FileWatcher fw = new FileWatcher();
            fw.Watch();
        }

        protected override void OnStop()
        {
        }
    }
}
