import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import { AuthProvider } from 'react-oidc-context'

const oidcConfig = {
  authority: "https://localhost:5443",
  client_id: "User_Investor",
  redirect_uri: "http://localhost:5173/signin-oidc",

};
ReactDOM.createRoot(document.getElementById('root')!).render(
  <AuthProvider {...oidcConfig}>
    <App />
  </AuthProvider>,
)
