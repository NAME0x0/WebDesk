from PyQt6.QtWidgets import (QDialog, QVBoxLayout, QPushButton, 
                           QCheckBox, QLineEdit, QLabel, QFileDialog)
from PyQt6.QtCore import pyqtSignal

class SettingsWindow(QDialog):
    settings_updated = pyqtSignal()
    
    def __init__(self, settings, parent=None):
        super().__init__(parent)
        self.settings = settings
        self.init_ui()
        
    def init_ui(self):
        self.setWindowTitle('WebDesk Settings')
        layout = QVBoxLayout()
        
        # Path input
        layout.addWidget(QLabel('Wallpaper HTML File:'))
        self.path_input = QLineEdit()
        self.path_input.setReadOnly(True)
        self.path_input.setText(self.settings.wallpaper_path)
        
        # Browse button
        browse_btn = QPushButton('Browse')
        browse_btn.clicked.connect(self.browse_file)
        
        # Autostart checkbox
        self.autostart_cb = QCheckBox('Start with Windows')
        self.autostart_cb.setChecked(self.settings.auto_start)
        
        # Save button
        save_btn = QPushButton('Save')
        save_btn.clicked.connect(self.save_settings)
        
        layout.addWidget(self.path_input)
        layout.addWidget(browse_btn)
        layout.addWidget(self.autostart_cb)
        layout.addWidget(save_btn)
        
        self.setLayout(layout)
        
    def browse_file(self):
        filename, _ = QFileDialog.getOpenFileName(
            self, 'Select HTML File', '',
            'HTML Files (*.html *.htm);;All Files (*.*)'
        )
        if filename:
            self.path_input.setText(filename)
            
    def save_settings(self):
        self.settings.wallpaper_path = self.path_input.text()
        self.settings.auto_start = self.autostart_cb.isChecked()
        self.settings.save()
        self.settings_updated.emit()
        self.accept()
