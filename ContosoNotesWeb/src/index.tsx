import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';

import {Msal2Provider} from '@microsoft/mgt-msal2-provider';
import {Providers} from '@microsoft/mgt-element';

Providers.globalProvider = new Msal2Provider({
  clientId: 'bb5531a6-103a-4682-99d4-625e3ddb6104',
  scopes: ['User.Read', 'Files.Read', 'Tasks.ReadWrite']
});


ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root')
);
