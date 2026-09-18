/**
 * =====================================================================
 * Arquivo....: ufs.ts
 * Versão.....: 1.1.0
 * Data.......: 18/09/2026
 * Descrição..: Lista das 27 unidades federativas (mesma lista validada
 *              pela API em UfAttribute.cs) e busca sem acentos.
 * ---------------------------------------------------------------------
 * Histórico de alterações:
 *   1.0.0 - 18/09/2026 - Criação do arquivo.
 *   1.1.0 - 18/09/2026 - Nome de cada UF, busca sem acentos e ordenação por relevância.
 * =====================================================================
 */

export interface Uf {
  sigla: string
  nome: string
}

export const UFS: readonly Uf[] = [
  { sigla: 'AC', nome: 'Acre' },
  { sigla: 'AL', nome: 'Alagoas' },
  { sigla: 'AP', nome: 'Amapá' },
  { sigla: 'AM', nome: 'Amazonas' },
  { sigla: 'BA', nome: 'Bahia' },
  { sigla: 'CE', nome: 'Ceará' },
  { sigla: 'DF', nome: 'Distrito Federal' },
  { sigla: 'ES', nome: 'Espírito Santo' },
  { sigla: 'GO', nome: 'Goiás' },
  { sigla: 'MA', nome: 'Maranhão' },
  { sigla: 'MT', nome: 'Mato Grosso' },
  { sigla: 'MS', nome: 'Mato Grosso do Sul' },
  { sigla: 'MG', nome: 'Minas Gerais' },
  { sigla: 'PA', nome: 'Pará' },
  { sigla: 'PB', nome: 'Paraíba' },
  { sigla: 'PR', nome: 'Paraná' },
  { sigla: 'PE', nome: 'Pernambuco' },
  { sigla: 'PI', nome: 'Piauí' },
  { sigla: 'RJ', nome: 'Rio de Janeiro' },
  { sigla: 'RN', nome: 'Rio Grande do Norte' },
  { sigla: 'RS', nome: 'Rio Grande do Sul' },
  { sigla: 'RO', nome: 'Rondônia' },
  { sigla: 'RR', nome: 'Roraima' },
  { sigla: 'SC', nome: 'Santa Catarina' },
  { sigla: 'SP', nome: 'São Paulo' },
  { sigla: 'SE', nome: 'Sergipe' },
  { sigla: 'TO', nome: 'Tocantins' },
]

function semAcentos(texto: string): string {
  return texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase()
}

/** Verdadeiro se o texto digitado aparece na sigla ou no nome da UF (ignora acentos). */
export function ufCorrespondeBusca(busca: string, uf: Uf): boolean {
  const termo = semAcentos(busca.trim())
  return semAcentos(uf.sigla).includes(termo) || semAcentos(uf.nome).includes(termo)
}

/**
 * Ordena os resultados da busca: sigla exata primeiro, depois nomes que começam
 * com o termo. Sem isso, "RN" traria Pernambuco (peRNambuco) antes do Rio Grande do Norte.
 */
export function compararRelevanciaUf(a: Uf, b: Uf, busca: string): number {
  const termo = semAcentos(busca.trim())
  const peso = (uf: Uf) => (semAcentos(uf.sigla) === termo ? 0 : semAcentos(uf.nome).startsWith(termo) ? 1 : 2)
  return peso(a) - peso(b)
}
