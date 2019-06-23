namespace DxfOpener.Properties {
    
    
    // Этот класс позволяет обрабатывать определенные события в классе параметров:
    //  Событие SettingChanging возникает перед изменением значения параметра.
    //  Событие PropertyChanged возникает после изменения значения параметра.
    //  Событие SettingsLoaded возникает после загрузки значений параметров.
    //  Событие SettingsSaving возникает перед сохранением значений параметров.
    internal sealed partial class Settings {
        
        public Settings() {
            // // Для добавления обработчиков событий для сохранения и изменения параметров раскомментируйте приведенные ниже строки:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        public WINDOWPLACEMENT WindowPlacement
        {
            get
            {
                return ((WINDOWPLACEMENT)(this["WindowPlacement"]));
            }
            set
            {
                this["WindowPlacement"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        public Profiles Profiles
        {
            get
            {
                return ((Profiles)(this["Profiles"]));
            }
            set
            {
                this["Profiles"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        public BarsPositions BarsPositions
        {
            get
            {
                return ((BarsPositions)(this["BarsPositions"]));
            }
            set
            {
                this["BarsPositions"] = value;
            }
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Добавьте здесь код для обработки события SettingChangingEvent.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Добавьте здесь код для обработки события SettingsSaving.
        }
    }
}
