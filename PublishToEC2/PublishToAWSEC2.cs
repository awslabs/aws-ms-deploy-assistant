using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublishToEC2
{
    //http://www.mztools.com/articles/2005/mz2005003.aspx
    //https://social.msdn.microsoft.com/Forums/vstudio/en-US/6e98eda6-b732-4850-b71d-732420523f18/vs2012-addin-how-to-add-a-solution-explorer-context-menuitem?forum=vsx
    public class PublishToAWSEC2 : IVsExtensibility2, IDTCommandTarget
    {
        public UIHierarchy BuildUIHierarchyFromTree(int hwnd, Window pParent)
        {
            throw new NotImplementedException();
        }

        public void EnterAutomationFunction()
        {
            throw new NotImplementedException();
        }

        public void Exec(string CmdName, vsCommandExecOption ExecuteOption, ref object VariantIn, ref object VariantOut, ref bool Handled)
        {
            throw new NotImplementedException();
        }

        public void ExitAutomationFunction()
        {
            throw new NotImplementedException();
        }

        public void FireCodeModelEvent(int dispid, CodeElement pElement, vsCMChangeKind changeKind)
        {
            throw new NotImplementedException();
        }

        public void FireCodeModelEvent3(int dispid, object pParent, CodeElement pElement, vsCMChangeKind changeKind)
        {
            throw new NotImplementedException();
        }

        public void FireMacroReset()
        {
            throw new NotImplementedException();
        }

        public void FireProjectItemsEvent_ItemAdded(ProjectItem ProjectItem)
        {
            throw new NotImplementedException();
        }

        public void FireProjectItemsEvent_ItemRemoved(ProjectItem ProjectItem)
        {
            throw new NotImplementedException();
        }

        public void FireProjectItemsEvent_ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            throw new NotImplementedException();
        }

        public void FireProjectsEvent_ItemAdded(Project Project)
        {
            throw new NotImplementedException();
        }

        public void FireProjectsEvent_ItemRemoved(Project Project)
        {
            throw new NotImplementedException();
        }

        public void FireProjectsEvent_ItemRenamed(Project Project, string OldName)
        {
            throw new NotImplementedException();
        }

        public ConfigurationManager GetConfigMgr(object pIVsProject, uint itemid)
        {
            throw new NotImplementedException();
        }

        public Document GetDocumentFromDocCookie(int lDocCookie)
        {
            throw new NotImplementedException();
        }

        public Globals GetGlobalsObject(object ExtractFrom)
        {
            throw new NotImplementedException();
        }

        public int GetLockCount()
        {
            throw new NotImplementedException();
        }

        public void GetSuppressUI(ref bool pOut)
        {
            throw new NotImplementedException();
        }

        public void GetUserControl(out bool fUserControl)
        {
            throw new NotImplementedException();
        }

        public void get_Properties(ISupportVSProperties pParent, object pdispPropObj, out Properties ppProperties)
        {
            throw new NotImplementedException();
        }

        public TextBuffer Get_TextBuffer(object pVsTextStream, IExtensibleObjectSite pParent)
        {
            throw new NotImplementedException();
        }

        public void IsFireCodeModelEventNeeded(ref bool vbNeeded)
        {
            throw new NotImplementedException();
        }

        public int IsInAutomationFunction()
        {
            throw new NotImplementedException();
        }

        public void IsMethodDisabled(ref Guid pGUID, int dispid)
        {
            throw new NotImplementedException();
        }

        public void LockServer(bool __MIDL_0010)
        {
            throw new NotImplementedException();
        }

        public void QueryStatus(string CmdName, vsCommandStatusTextWanted NeededText, ref vsCommandStatus StatusOption, ref object CommandText)
        {
            throw new NotImplementedException();
        }

        public wizardResult RunWizardFile(string bstrWizFilename, int hwndOwner, ref object[] vContextParams)
        {
            throw new NotImplementedException();
        }

        public int RunWizardFileEx(string bstrWizFilename, int hwndOwner, ref object[] vContextParams, ref object[] vCustomParams)
        {
            throw new NotImplementedException();
        }

        public void SetSuppressUI(bool In)
        {
            throw new NotImplementedException();
        }

        public void SetUserControl(bool fUserControl)
        {
            throw new NotImplementedException();
        }

        public void SetUserControlUnlatched(bool fUserControl)
        {
            throw new NotImplementedException();
        }

        public bool TestForShutdown()
        {
            throw new NotImplementedException();
        }
    }
}
