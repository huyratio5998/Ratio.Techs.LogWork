using Ratio.LogWork.Entity;

namespace Ratio.LogWork.Service.HandleCommands
{
    public class HandleCommandFactory
    {
        public IHandleCommands Create(WorkLogHistoryAction command)
        {
            IHandleCommands result = new HandleNoActionCommand();
            switch (command)
            {
                case WorkLogHistoryAction.Start:
                    result = new HandleStartCommand();
                    break;
                case WorkLogHistoryAction.Pause:
                    result = new HandlePauseCommand();
                    break;
                case WorkLogHistoryAction.Continue:
                    result = new HandleContinueCommand();
                    break;
                case WorkLogHistoryAction.Done:
                    result = new HandleDoneCommand();
                    break;
                case WorkLogHistoryAction.Cancel:
                    result = new HandleCancelCommand();
                    break;
                case WorkLogHistoryAction.NoAction:
                    result = new HandleNoActionCommand();
                    break;
            }

            return result;
        }
    }
}
