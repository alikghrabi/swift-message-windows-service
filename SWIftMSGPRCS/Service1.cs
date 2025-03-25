using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using SWIftMSGPRCS.Func1;
using SWIftMSGPRCS.Func2;

namespace SWIftMSGPRCS
{
    public partial class Service1 : ServiceBase
    {
        private Timer _timer;
        private SwiftMsgPrcs _swiftProcessor;
        private MappingReferenceUsers _mappingProcessor;
        private string _watchPath = @"C:\Users\Ali Ghrabi\OneDrive\Desktop\AEG_Training_Asp\";
        private string _successPath = @"C:\Users\Ali Ghrabi\OneDrive\Desktop\AEG_Training_Asp\Success";
        private string _errorPath = @"C:\Users\Ali Ghrabi\OneDrive\Desktop\AEG_Training_Asp\Error";
        private string _connectionString = "Server=DESKTOP-S2P142E\\SQLEXPRESS;Database=AEG_Training_Asp;Trusted_Connection=True;TrustServerCertificate=True;";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _swiftProcessor = new SwiftMsgPrcs(_watchPath, _successPath, _errorPath, _connectionString);
            _mappingProcessor = new MappingReferenceUsers(_connectionString);

            _timer = new Timer(10000);
            _timer.Elapsed += OnTimedEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            try
            {
                _swiftProcessor.ProcessSwiftMessages();
                _mappingProcessor.ProcessMessages();
            }
            catch (Exception ex)
            {
                File.AppendAllText(_errorPath + @"\ServiceErrors.txt", $"[{DateTime.Now}] Error: {ex.Message}\n");
            }
        }

        protected override void OnStop()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
