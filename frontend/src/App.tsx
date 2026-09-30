import { Route, Routes } from 'react-router-dom';
import { DetalhesPage } from './pages/DetalhesPage';
import { ListagemPage } from './pages/ListagemPage';
import { NovoCandidatoPage } from './pages/NovoCandidatoPage';

function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <Routes>
        <Route path="/" element={<ListagemPage />} />
        <Route path="/candidatos/novo" element={<NovoCandidatoPage />} />
        <Route path="/candidatos/:id" element={<DetalhesPage />} />
      </Routes>
    </div>
  );
}

export default App;
