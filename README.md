# YLibrary Framework Persistence

## Introdução.
O YLibrary é um entity framework de persistência, utilizando-se Entidades do projeto para persistir em tabelas do banco de dados gerenciadas pelo próprio framework.

Para um entendimento mais completo da estrutura, funcionaneneto e exemplos praticos, pode ser consultado a documentação completa no diretorio do projeto Documentos/Documentação completa da YLibrary.pdf

## Iniciando

1. Base de Dados
- Primeiramente deverá ser criado um Database no banco de dados a sua escolha (Lembres-se de utilizar o YLibrary respectivo ao Banco de dados), que irá conter as tabelas de seu projeto e no web.config do seu projeto uma connectionstring nomeada de YLibrary, com as informações de conexão para esse banco de dados criado anteriormente.
	- Projeto
A YLibrary trabalha com entidades, independente da estrutura de seu projeto, porem recomendamos a utilização do padrão adotado na criação do YLibrary.
O Padrão YLibrary utiliza uma class library no projeto para as Entidades e outra para Facade, tento a YLibrary referenciado no Facade.
	- Entidade
A class library Entidade deverá conter as entidades de seu projeto, podemos entender como entidade uma tabela do banco de dados. As entidades terão que utilizar o padrão Pascal Case junto com o padrão de nomenclatura necessário para o gerenciamento das entidades com as tabelas do banco, segue abaixo as regras,
		- Os três primeiros caracteres do nome da entidade deverão ser obrigatoriamente:

			- Dsc: Varchar(255).
			- Flg: Varchar(5).
			- Dth: Datetime.
			- Vlr: Decimal.
			- Num: int.
			- Isn: Chave Primaria.

		- O nome da entidade devera seguir o padrão Pascal Case:
			- DscTeste.
			- IsnTeste.

		- Todos os tipos deverão ser nullables.
			- int? IsnTeste.
			- datetime? DthTeste.

		- O nome da Entidade deverá seguir o padrão Pascal Case, sempre iniciando com a letra E.
			- ETeste.

## Facade

1. A class library Facade deverá conter as regras de persistência, entre outras regras necessárias para validações dos dados, como também a referencia a YLibrary. Deverá ser criado um Facade para cada Entidade, onde esse Facade irá receber a entidade da camada de visão e irá persistir no banco de dados através da instancia do YLibrary. O Facade também deverá seguir alguns padrões listados logo abaixo,
	- O Nome do Facade deverá seguir o padrão Pascal Case, sempre iniciando com a letra F.
		- FTeste
		
## YLibrary

1. A YLibrary irá criar as tabelas no banco de dados baseado nas Entidades existentes no Projeto, na chamada de qualquer método da classe, ele irá verificar a existência da tabela, caso não exista ele cria e depois excuta o método solicitado. 
Caso a tabela já tenha sido criado pela YLibrary, mas foi criado uma nova coluna na entidade, a YLibrary irá adicionar essa coluna na tabela.
No caso de exclusão de campos da entidade, a YLibrary não irá excluir a coluna do banco de dados, caso ela já tenha sido criada.
Os parâmetros recebidos nos métodos serão sempre Objects, pois o YLibrary irá utilizar Reflection para identificar as entidades.
Abaixo temos exemplo da utilização dos métodos da YLibrary,

	- Void Insert(Object o)
		- YLibrary conn = new YLibrary();
		- ETeste entidade = new ETeste(1,"Teste");
		- conn.Insert(entidade);

	- Void Update(Object o)
		- YLibrary conn = new YLibrary();
		- ETeste entidade = new ETeste(1,"Teste");
		- conn.Update(entidade);

	- Void Delete(Object o)
		- YLibrary conn = new YLibrary();
		- ETeste entidade = new ETeste(1,"Teste");
		- conn.Delete(entidade);

	- Void Select(Object o)
		- YLibrary conn = new YLibrary();
		- ETeste entidade = new ETeste(1,"Teste");
		- List<ETeste> lst = conn.Select(entidade).Cast<ETeste>().ToList();

	- Void Select(String sql)
		- YLibrary conn = new YLibrary();
		- Sql = “Select isn_teste, dsc_teste from Tb_teste”
		- List<Object[]> lst = lst conn.Select(sql) .Cast<Object[]>().ToList();



